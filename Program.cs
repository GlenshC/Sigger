using Sigger.Data;
using Sigger.NeuralNetwork;
using Sigger.Math;
using System.Diagnostics;
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
        private const string ModelCachePath = "model_cache.json";

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

            // If no CLI args or --gui specified, run Windows Forms GUI
            if (!runTrain && !runTest && !runBenchmark && !forceGui && args.Length == 0)
            {
                RunGui();
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
                RunCli(args, runTrain, runTest, epochs, learningRate, batchSize, hiddenSize);
                return;
            }

            RunGui();
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

        static void RunGui()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void RunCli(string[] args, bool runTrain, bool runTest, int epochs, double learningRate, int batchSize, int hiddenSize)
        {
            Console.WriteLine("=== Sigger CLI ===");
            Console.WriteLine();

            // Load data
            Console.WriteLine("Loading dataset...");
            var sw = Stopwatch.StartNew();
            var dataset = DataLoader.Load(TrainCsvPath, TestCsvPath, valSplit: 0.1, seed: 42);
            sw.Stop();
            Console.WriteLine($"Data loaded in {sw.ElapsedMilliseconds}ms:");
            Console.WriteLine($"  Train: {dataset.TrainX.GetLength(0)} samples");
            Console.WriteLine($"  Val:   {dataset.ValX.GetLength(0)} samples");
            Console.WriteLine($"  Test:  {dataset.TestX.GetLength(0)} samples");
            Console.WriteLine($"  Classes: {dataset.NumClasses}");
            Console.WriteLine();

            // Create or load model
            MLP model;
            if (File.Exists(ModelCachePath) && !runTrain)
            {
                Console.WriteLine("Loading cached model...");
                model = MLP.Load(ModelCachePath);
                Console.WriteLine($"Model: {model.InputSize} → {model.HiddenSize} → {model.OutputSize}");
            }
            else
            {
                model = new MLP(dataset.InputSize, hiddenSize, dataset.NumClasses, seed: 42);
                Console.WriteLine($"Model created: {dataset.InputSize} → {hiddenSize} → {dataset.NumClasses}");
                Console.WriteLine($"Hyperparams: LR={learningRate}, Batch={batchSize}, Epochs={epochs}");
                Console.WriteLine();

                // Train
                Console.WriteLine("Starting training...");
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
                        Console.WriteLine($"  {msg}");
                    },
                    earlyStopPatience: 15
                );
                sw.Stop();
                Console.WriteLine($"\nTraining complete in {sw.Elapsed.TotalSeconds:F1}s");
                Console.WriteLine($"Final loss: {losses[^1]:F4}");

                // Save cache
                model.Save(ModelCachePath);
                Console.WriteLine($"Model saved to {ModelCachePath}");
            }

            if (runTest || args.Length == 0)
            {
                Console.WriteLine();
                Console.WriteLine("=== Evaluation ===");

                // Validation accuracy
                sw.Restart();
                double valAcc = model.EvaluateAccuracy(dataset.ValX, dataset.ValLabels);
                sw.Stop();
                Console.WriteLine($"Validation Accuracy: {valAcc:P2} (in {sw.ElapsedMilliseconds}ms)");

                // Test accuracy
                sw.Restart();
                double testAcc = model.EvaluateAccuracy(dataset.TestX, dataset.TestLabels);
                sw.Stop();
                Console.WriteLine($"Test Accuracy: {testAcc:P2} (in {sw.ElapsedMilliseconds}ms)");

                // Per-class metrics
                var cm = model.ConfusionMatrix(dataset.TestX, dataset.TestLabels);
                Console.WriteLine("\nPer-Class Test Accuracy:");
                for (int i = 0; i < dataset.NumClasses; i++)
                {
                    int total = 0, correct = 0;
                    for (int j = 0; j < dataset.NumClasses; j++)
                    {
                        total += cm[i, j];
                        if (i == j) correct = cm[i, j];
                    }
                    if (total > 0)
                        Console.WriteLine($"  {IndexToLabel(i)}: {correct}/{total} = {correct / (double)total:P1}");
                }
            }
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
    }
}