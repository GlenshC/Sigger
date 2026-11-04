using Sigger.Data;
using Sigger.NeuralNetwork;
using Sigger.Math;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace Sigger
{
    /// <summary>
    /// Entry point for Sigger - runs as GUI by default, or CLI with arguments.
    /// Usage: dotnet run [--train] [--test] [--bench] [--gui] [--epochs N] [--lr N] [--batch N] [--hidden N]
    /// </summary>
    internal static class Program
    {
        private const string TrainCsvPath = @"data\mnist_train.csv";
        private const string TestCsvPath = @"data\mnist_test.csv";
        private const string ModelCacheDir = "model_cache";
        private const string ResultsDir = "results";

        [STAThread]
        static void Main(string[] args)
        {
            bool runTest = args.Contains("--test");
            bool runTrain = args.Contains("--train");
            bool runBenchmark = args.Contains("--bench");
            bool forceGui = args.Contains("--gui");

            // Parse optional hyperparameters
            int epochs = ParseArg(args, "--epochs", 100);
            double learningRate = ParseArg(args, "--lr", 0.01);
            int batchSize = ParseArg(args, "--batch", 64);
            int hiddenSize = ParseArg(args, "--hidden", 256);
            string modelPath = ParseArg(args, "--model", "");

            // If no CLI args or --gui specified, run Windows Forms GUI
            if (!runTrain && !runTest && !runBenchmark && !forceGui && args.Length == 0)
            {
                RunGui(modelPath);
                return;
            }

            // CLI mode
            if (runBenchmark)
            {
                RunBenchmark();
                return;
            }

            if (runTrain || runTest || forceGui || args.Length > 0)
            {
                RunCli(args, runTrain, runTest, epochs, learningRate, batchSize, hiddenSize, modelPath);
                return;
            }

            RunGui(modelPath);
        }

        static int ParseArg(string[] args, string name, int defaultValue)
        {
            int idx = Array.IndexOf(args, name);
            if (idx >= 0 && idx + 1 < args.Length && int.TryParse(args[idx + 1], out int val))
                return val;
            return defaultValue;
        }

        static double ParseArg(string[] args, string name, double defaultValue)
        {
            int idx = Array.IndexOf(args, name);
            if (idx >= 0 && idx + 1 < args.Length && double.TryParse(args[idx + 1], out double val))
                return val;
            return defaultValue;
        }

        static string ParseArg(string[] args, string name, string defaultValue)
        {
            int idx = Array.IndexOf(args, name);
            if (idx >= 0 && idx + 1 < args.Length)
                return args[idx + 1];
            return defaultValue;
        }

        static void RunGui(string modelPath = "")
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(modelPath));
        }

        static void RunCli(string[] args, bool runTrain, bool runTest, int epochs, double learningRate, int batchSize, int hiddenSize, string modelPath = "")
        {
            var log = new List<LogEntry>();
            string phase = runTrain ? "TRAIN" : "TEST";

            // Redirect console output to capture logs
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Also write to console in real-time
            var teeWriter = new TeeWriter(originalOut, stringWriter);
            Console.SetOut(teeWriter);

            void Log(string message, string level = "INFO")
            {
                var entry = new LogEntry
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                    Level = level,
                    Message = message
                };
                log.Add(entry);
                Console.WriteLine(message);
            }

            try
            {
                Log("=== Sigger CLI ===");
                Log($"Hyperparams: LR={learningRate}, Hidden={hiddenSize}, Batch={batchSize}, Epochs={epochs}");

                // Load data
                Log("Loading dataset...");
                var sw = Stopwatch.StartNew();
                var dataset = DataLoader.Load(TrainCsvPath, TestCsvPath, valSplit: 0.1, seed: 42);
                sw.Stop();
                Log($"Data loaded in {sw.ElapsedMilliseconds}ms:");
                Log($"  Train: {dataset.TrainX.GetLength(0)} samples");
                Log($"  Val:   {dataset.ValX.GetLength(0)} samples");
                Log($"  Test:  {dataset.TestX.GetLength(0)} samples");
                Log($"  Classes: {dataset.NumClasses}");

                // Generate cache path based on hyperparameters
                string lrStr = learningRate.ToString("G17").Replace(".", "p").Replace("E", "e").Replace("+", "").Replace("-", "n");
                string modelCachePath = Path.Combine(ModelCacheDir, $"model_cache_{lrStr}_{hiddenSize}_{batchSize}_{epochs}.json");

                // Determine which model to load
                string loadPath = !string.IsNullOrEmpty(modelPath) ? modelPath : modelCachePath;

                // Create or load model
                MLP model;
                if (File.Exists(loadPath) && !runTrain)
                {
                    Log($"Loading model from {loadPath}...");
                    model = MLP.Load(loadPath);
                    Log($"Model: {model.InputSize} → {model.HiddenSize} → {model.OutputSize}");
                }
                else
                {
                    if (!string.IsNullOrEmpty(modelPath) && !File.Exists(modelPath))
                    {
                        Log($"WARNING: Specified model not found: {modelPath}", "WARN");
                    }
                    model = new MLP(dataset.InputSize, hiddenSize, dataset.NumClasses, seed: 42);
                    Log($"Model created: {dataset.InputSize} → {hiddenSize} → {dataset.NumClasses}");

                    // Train
                    Log("Starting training...");
                    sw.Restart();
                    var losses = model.Train(
                        dataset.TrainX, dataset.TrainY,
                        epochs, batchSize, learningRate,
                        dataset.ValX, dataset.ValY,
                        onEpochEnd: (epoch, loss, valLoss) =>
                        {
                            var msg = valLoss.HasValue
                                ? $"Epoch {epoch:D3}/{epochs}: Loss={loss:F4}, ValLoss={valLoss.Value:F4}"
                                : $"Epoch {epoch:D3}/{epochs}: Loss={loss:F4}";
                            Log($"  {msg}");
                        },
                        earlyStopPatience: 15
                    );
                    sw.Stop();
                    Log($"\nTraining complete in {sw.Elapsed.TotalSeconds:F1}s");
                    Log($"Final loss: {losses[^1]:F4}");

                    // Save cache
                    Directory.CreateDirectory(ModelCacheDir);
                    model.Save(modelCachePath);
                    Log($"Model saved to {modelCachePath}");
                }

                if (runTest || (!runTrain && args.Length == 0))
                {
                    Log("");
                    Log("=== Evaluation ===");

                    // Validation accuracy
                    sw.Restart();
                    double valAcc = model.EvaluateAccuracy(dataset.ValX, dataset.ValLabels);
                    sw.Stop();
                    Log($"Validation Accuracy: {valAcc:P2} (in {sw.ElapsedMilliseconds}ms)");

                    // Test accuracy
                    sw.Restart();
                    double testAcc = model.EvaluateAccuracy(dataset.TestX, dataset.TestLabels);
                    sw.Stop();
                    Log($"Test Accuracy: {testAcc:P2} (in {sw.ElapsedMilliseconds}ms)");

                    // Per-class metrics
                    var cm = model.ConfusionMatrix(dataset.TestX, dataset.TestLabels);
                    Log("\nPer-Class Test Accuracy:");
                    var perClass = new List<PerClassMetric>();
                    for (int i = 0; i < dataset.NumClasses; i++)
                    {
                        int total = 0, correct = 0;
                        for (int j = 0; j < dataset.NumClasses; j++)
                        {
                            total += cm[i, j];
                            if (i == j) correct = cm[i, j];
                        }
                        if (total > 0)
                        {
                            double acc = correct / (double)total;
                            Log($"  {IndexToLabel(i)}: {correct}/{total} = {acc:P1}");
                            perClass.Add(new PerClassMetric
                            {
                                Class = IndexToLabel(i),
                                Correct = correct,
                                Total = total,
                                Accuracy = acc
                            });
                        }
                    }

                    // Save results to JSON
                    SaveResults(phase, learningRate, hiddenSize, batchSize, epochs, log, perClass, valAcc, testAcc);
                }
                else if (runTrain)
                {
                    // Save training-only results
                    SaveResults(phase, learningRate, hiddenSize, batchSize, epochs, log, new List<PerClassMetric>(), 0, 0);
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}", "ERROR");
                Log(ex.StackTrace ?? "", "ERROR");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        static void SaveResults(string phase, double lr, int hidden, int batch, int epochs, List<LogEntry> log, List<PerClassMetric> perClass, double valAcc, double testAcc)
        {
            Directory.CreateDirectory(ResultsDir);

            string lrStr = lr.ToString("G17").Replace(".", "p").Replace("E", "e").Replace("+", "").Replace("-", "n");
            string fileName = $"{lrStr}_{hidden}_{batch}_{epochs}_{phase}.json";
            string filePath = Path.Combine(ResultsDir, fileName);

            var result = new CliResult
            {
                Phase = phase,
                Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                Hyperparameters = new Hyperparameters
                {
                    LearningRate = lr,
                    HiddenSize = hidden,
                    BatchSize = batch,
                    Epochs = epochs
                },
                Metrics = new Metrics
                {
                    ValidationAccuracy = valAcc,
                    TestAccuracy = testAcc,
                    PerClass = perClass
                },
                Logs = log
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(result, options);
            File.WriteAllText(filePath, json);

            Console.WriteLine($"\nResults saved to: {filePath}");
        }

        static void RunBenchmark()
        {
            Console.WriteLine("=== Matrix Ops Benchmark ===");
            var rng = new Random(42);

            // Benchmark different matrix sizes
            var sizes = new (int m, int n, int p)[]
            {
                (64, 784, 256),   // Input → Hidden
                (64, 256, 24),    // Hidden → Output
                (256, 784, 64),   // Transposed
                (256, 256, 256),  // Square
            };

            foreach (var (m, n, p) in sizes)
            {
                var a = MatrixOps.Random(m, n, rng: rng);
                var b = MatrixOps.Random(n, p, rng: rng);

                // Warmup
                for (int i = 0; i < 3; i++)
                    _ = MatrixOps.MultiplyParallel(a, b);

                // Benchmark
                var sw = Stopwatch.StartNew();
                int iterations = 100;
                for (int i = 0; i < iterations; i++)
                    _ = MatrixOps.MultiplyParallel(a, b);
                sw.Stop();

                double msPerOp = sw.Elapsed.TotalMilliseconds / iterations;
                double gflops = (2.0 * m * n * p * iterations) / (sw.Elapsed.TotalSeconds * 1e9);
                Console.WriteLine($"{m}x{n} × {n}x{p}: {msPerOp:F2}ms/op, {gflops:F2} GFLOPS");
            }

            Console.WriteLine($"\nAVX2 Supported: {System.Runtime.Intrinsics.X86.Avx2.IsSupported}");
            Console.WriteLine($"Processor Count: {Environment.ProcessorCount}");
        }

        static string IndexToLabel(int idx)
        {
            return idx.ToString();
        }

        // Tee writer to write to both console and string buffer
        private class TeeWriter : TextWriter
        {
            private readonly TextWriter _writer1;
            private readonly TextWriter _writer2;

            public TeeWriter(TextWriter writer1, TextWriter writer2)
            {
                _writer1 = writer1;
                _writer2 = writer2;
            }

            public override Encoding Encoding => _writer1.Encoding;

            public override void Write(char value)
            {
                _writer1.Write(value);
                _writer2.Write(value);
            }

            public override void Write(string? value)
            {
                _writer1.Write(value);
                _writer2.Write(value);
            }

            public override void Flush()
            {
                _writer1.Flush();
                _writer2.Flush();
            }
        }

        // JSON serialization DTOs
        private class CliResult
        {
            public string Phase { get; set; } = "";
            public string Timestamp { get; set; } = "";
            public Hyperparameters Hyperparameters { get; set; } = new();
            public Metrics Metrics { get; set; } = new();
            public List<LogEntry> Logs { get; set; } = new();
        }

        private class Hyperparameters
        {
            public double LearningRate { get; set; }
            public int HiddenSize { get; set; }
            public int BatchSize { get; set; }
            public int Epochs { get; set; }
        }

        private class Metrics
        {
            public double ValidationAccuracy { get; set; }
            public double TestAccuracy { get; set; }
            public List<PerClassMetric> PerClass { get; set; } = new();
        }

        private class PerClassMetric
        {
            public string Class { get; set; } = "";
            public int Correct { get; set; }
            public int Total { get; set; }
            public double Accuracy { get; set; }
        }

        private class LogEntry
        {
            public string Timestamp { get; set; } = "";
            public string Level { get; set; } = "INFO";
            public string Message { get; set; } = "";
        }
    }
}