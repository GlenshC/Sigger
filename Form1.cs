using System.Drawing;
using System.Text;
using Sigger.Data;
using Sigger.NeuralNetwork;
using SysMath = System.Math;

namespace Sigger
{
    public partial class Form1 : Form
    {
        private const string ModelCacheDir = "model_cache";
        private const string DefaultModelCachePath = "model_cache/model_cache_0p01_256_64_100.json";
        private const string TrainCsvPath = @"data\mnist_train.csv";
        private const string TestCsvPath = @"data\mnist_test.csv";

        private Dataset? _dataset;
        private MLP? _model;
        private CancellationTokenSource? _trainingCts;
        private Bitmap? _drawCanvas;
        private Graphics? _drawGraphics;
        private bool _isDrawing = false;
        private Point _lastPoint;
        private const int CanvasSize = 280; // 10x scale for 28x28
        private int _drawRadius = 12; // Brush radius (adjustable, default 12px ≈ 1.2px at 28x28)

        public Form1() : this("")
        {
        }

        public Form1(string modelPath)
        {
            InitializeComponent();
            WireEvents();
            InitializeDrawCanvas();

            if (!string.IsNullOrEmpty(modelPath) && File.Exists(modelPath))
            {
                LoadSpecificModel(modelPath);
            }
            else
            {
                LoadCacheIfExists();
            }

            RefreshModelList();
        }

        private void LoadCacheIfExists()
        {
            // Try to load the default cache first, then fall back to any cache file
            string[] cacheFiles = new[] { DefaultModelCachePath };
            if (Directory.Exists(ModelCacheDir))
            {
                cacheFiles = cacheFiles.Concat(Directory.GetFiles(ModelCacheDir, "model_cache_*.json")).ToArray();
            }

            foreach (var cachePath in cacheFiles)
            {
                if (File.Exists(cachePath))
                {
                    try
                    {
                        _model = MLP.Load(cachePath);
                        Log($"Loaded cached model: {_model.InputSize}→{_model.HiddenSize}→{_model.OutputSize} from {cachePath}");
                        btnTest.Enabled = true;
                        return;
                    }
                    catch (Exception ex)
                    {
                        Log($"Failed to load cache {cachePath}: {ex.Message}");
                    }
                }
            }
        }

        private void LoadSpecificModel(string modelPath)
        {
            try
            {
                _model = MLP.Load(modelPath);
                Log($"Loaded model: {_model.InputSize}→{_model.HiddenSize}→{_model.OutputSize} from {modelPath}");
                btnTest.Enabled = true;
            }
            catch (Exception ex)
            {
                Log($"Failed to load model {modelPath}: {ex.Message}");
            }
        }

        private void RefreshModelList()
        {
            if (cmbModelCache == null) return;

            var files = new List<string>();
            if (Directory.Exists(ModelCacheDir))
            {
                files.AddRange(Directory.GetFiles(ModelCacheDir, "model_cache_*.json")
                    .OrderByDescending(f => File.GetLastWriteTime(f)));
            }

            cmbModelCache.Items.Clear();
            cmbModelCache.Items.Add("(Auto-detect latest)");
            foreach (var f in files)
            {
                cmbModelCache.Items.Add(f);
            }
            cmbModelCache.SelectedIndex = 0;
        }

        private void WireEvents()
        {
            // Train tab
            btnTrain.Click += BtnTrain_Click;
            btnTest.Click += BtnTest_Click;
            btnClearCache.Click += BtnClearCache_Click;

            // Test tab
            btnEvaluate.Click += BtnEvaluate_Click;

            // Draw Test tab
            btnClearCanvas.Click += BtnClearCanvas_Click;
            picDrawCanvas.MouseDown += PicDrawCanvas_MouseDown;
            picDrawCanvas.MouseMove += PicDrawCanvas_MouseMove;
            picDrawCanvas.MouseUp += PicDrawCanvas_MouseUp;

            // Form closing
            FormClosing += Form1_FormClosing;
        }

        private void cmbModelCache_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selected = cmbModelCache.SelectedItem as string;
            if (string.IsNullOrEmpty(selected)) return;

            if (selected != "(Auto-detect latest)" && File.Exists(selected))
            {
                LoadSpecificModel(selected);
            }
            else if (selected == "(Auto-detect latest)")
            {
                LoadCacheIfExists();
            }
        }

        private void InitializeDrawCanvas()
        {
            _drawCanvas = new Bitmap(CanvasSize, CanvasSize);
            _drawGraphics = Graphics.FromImage(_drawCanvas);
            _drawGraphics.Clear(Color.White);
            picDrawCanvas.Image = _drawCanvas;

            // Wire brush size control
            numBrushSize.ValueChanged += (s, e) => _drawRadius = (int)numBrushSize.Value;
        }

        #region Draw Test Tab

        private void BtnClearCanvas_Click(object? sender, EventArgs e)
        {
            if (_drawGraphics != null)
            {
                _drawGraphics.Clear(Color.White);
                picDrawCanvas.Invalidate();
            }
            lblDrawPrediction.Text = "Draw a digit (0-9)";
            lblDrawPrediction.ForeColor = Color.Gray;
            txtDrawConfidence.Clear();
        }

        private void PicDrawCanvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDrawing = true;
                _lastPoint = e.Location;
                DrawAt(e.Location);
            }
        }

        private void PicDrawCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDrawing && e.Button == MouseButtons.Left)
            {
                DrawLine(_lastPoint, e.Location);
                _lastPoint = e.Location;
            }
        }

        private void PicDrawCanvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _isDrawing = false;
                PredictDrawing();
            }
        }

        private void DrawAt(Point p)
        {
            if (_drawGraphics == null) return;
            using var brush = new SolidBrush(Color.Black);
            int r = _drawRadius;
            _drawGraphics.FillEllipse(brush, p.X - r, p.Y - r, r * 2, r * 2);
            picDrawCanvas.Invalidate();
        }

        private void DrawLine(Point p1, Point p2)
        {
            if (_drawGraphics == null) return;
            using var pen = new Pen(Color.Black, _drawRadius * 2)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            _drawGraphics.DrawLine(pen, p1, p2);
            picDrawCanvas.Invalidate();
        }

        private async void PredictDrawing()
        {
            if (_model == null || _drawCanvas == null)
            {
                lblDrawPrediction.Text = "Train or load a model first";
                lblDrawPrediction.ForeColor = Color.Red;
                return;
            }

            try
            {
                // Preprocess: resize 280x280 -> 28x28, normalize
                var input = PreprocessDrawing(_drawCanvas);
                var inputMatrix = new double[1, 784];
                for (int i = 0; i < 784; i++) inputMatrix[0, i] = input[i];

                var probs = _model.PredictProba(inputMatrix);
                int pred = _model.Predict(inputMatrix)[0];
                double confidence = probs[0, pred];

                lblDrawPrediction.Text = $"Prediction: {pred}";
                lblDrawPrediction.ForeColor = confidence > 0.7 ? Color.Green : Color.Orange;

                // Show all class probabilities
                var sb = new StringBuilder();
                sb.AppendLine($"Digit: {pred}  |  Confidence: {confidence:P2}");
                sb.AppendLine();
                sb.AppendLine("All probabilities:");
                for (int i = 0; i < 10; i++)
                {
                    sb.AppendLine($"  {i}: {probs[0, i]:P2}");
                }
                txtDrawConfidence.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                lblDrawPrediction.Text = $"Error: {ex.Message}";
                lblDrawPrediction.ForeColor = Color.Red;
            }
        }

        private double[] PreprocessDrawing(Bitmap canvas)
        {
            // Resize to 28x28 using high-quality bicubic
            using var resized = new Bitmap(28, 28);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(canvas, 0, 0, 28, 28);
            }

            // Convert to grayscale and normalize [0,1]
            // MNIST expects white digits on black background (white=1, black=0)
            // Our canvas is black digits on white background, so invert: 1.0 - gray
            var input = new double[784];
            int idx = 0;
            for (int y = 0; y < 28; y++)
            {
                for (int x = 0; x < 28; x++)
                {
                    var pixel = resized.GetPixel(x, y);
                    // Grayscale: 0.299*R + 0.587*G + 0.114*B
                    double gray = (0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B) / 255.0;
                    input[idx++] = 1.0 - gray; // Invert: MNIST has white-on-black
                }
            }
            return input;
        }

        #endregion

        #region Train Tab

        private async void BtnTrain_Click(object? sender, EventArgs e)
        {
            if (_trainingCts != null) return; // Already training

            btnTrain.Enabled = false;
            btnTest.Enabled = false;
            btnClearCache.Enabled = false;
            progressBarTraining.Value = 0;
            txtLog.Clear();

            _trainingCts = new CancellationTokenSource();
            var token = _trainingCts.Token;

            try
            {
                // Load data on background thread to avoid UI freeze
                Log("Loading dataset...");
                _dataset = await Task.Run(() => DataLoader.Load(TrainCsvPath, TestCsvPath, valSplit: 0.1, seed: 42), token);
                Log($"Data loaded: Train={_dataset.TrainX.GetLength(0)}, Val={_dataset.ValX.GetLength(0)}, Test={_dataset.TestX.GetLength(0)}");

                // Create model with UI hyperparameters
                int hiddenSize = (int)numHiddenSize.Value;
                double learningRate = (double)numLearningRate.Value;
                int batchSize = (int)numBatchSize.Value;
                int epochs = (int)numEpochs.Value;

                _model = new MLP(_dataset.InputSize, hiddenSize, _dataset.NumClasses, seed: 42);
                Log($"Model created: {_dataset.InputSize} → {hiddenSize} → {_dataset.NumClasses}");
                Log($"Hyperparams: LR={learningRate}, Batch={batchSize}, Epochs={epochs}");

                // Train on background thread
                Log("Starting training...");
                var losses = await Task.Run(() => _model.Train(
                    _dataset.TrainX, _dataset.TrainY,
                    epochs, batchSize, learningRate,
                    _dataset.ValX, _dataset.ValY,
                    onEpochEnd: (epoch, loss, valLoss) =>
                    {
                        if (token.IsCancellationRequested) return;
                        var msg = valLoss.HasValue
                            ? $"Epoch {epoch:D3}/{epochs}: Loss={loss:F4}, ValLoss={valLoss.Value:F4}"
                            : $"Epoch {epoch:D3}/{epochs}: Loss={loss:F4}";
                        // Use BeginInvoke for thread-safe UI updates
                        BeginInvoke(new Action(() =>
                        {
                            Log(msg);
                            UpdateProgress(epoch, epochs);
                        }));
                    },
                    earlyStopPatience: 15,
                    cancellationToken: token
                ), token);

                // Save cache with hyperparameter-specific filename
                string lrStr = learningRate.ToString("G17").Replace(".", "p").Replace("E", "e").Replace("+", "").Replace("-", "n");
                string cachePath = Path.Combine(ModelCacheDir, $"model_cache_{lrStr}_{hiddenSize}_{batchSize}_{epochs}.json");
                Directory.CreateDirectory(ModelCacheDir);
                _model.Save(cachePath);
                Log($"Training complete. Model saved to {cachePath}");

                btnTest.Enabled = true;
            }
            catch (OperationCanceledException)
            {
                Log("Training cancelled.");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                Log(ex.StackTrace ?? "");
            }
            finally
            {
                _trainingCts = null;
                btnTrain.Enabled = true;
                btnClearCache.Enabled = true;
                progressBarTraining.Value = 0;
            }
        }

        private async void BtnTest_Click(object? sender, EventArgs e)
        {
            if (_model == null)
            {
                Log("No model loaded. Train first or load from cache.");
                return;
            }

            // Load dataset if model was loaded from cache
            if (_dataset == null)
            {
                Log("Loading dataset for evaluation...");
                _dataset = await Task.Run(() => DataLoader.Load(TrainCsvPath, TestCsvPath, valSplit: 0.1, seed: 42));
                Log($"Data loaded: Train={_dataset.TrainX.GetLength(0)}, Val={_dataset.ValX.GetLength(0)}, Test={_dataset.TestX.GetLength(0)}");
            }

            btnTest.Enabled = false;
            Log("Running quick test on validation set...");

            try
            {
                double acc = await Task.Run(() => _model.EvaluateAccuracy(_dataset.ValX, _dataset.ValLabels));
                Log($"Validation Accuracy: {acc:P2}");

                // Show per-class accuracy
                var cm = await Task.Run(() => _model.ConfusionMatrix(_dataset.ValX, _dataset.ValLabels));
                Log("Per-class accuracy:");
                for (int i = 0; i < _dataset.NumClasses; i++)
                {
                    int total = 0, correct = 0;
                    for (int j = 0; j < _dataset.NumClasses; j++)
                    {
                        total += cm[i, j];
                        if (i == j) correct = cm[i, j];
                    }
                    if (total > 0)
                        Log($"  {IndexToLabel(i)}: {correct}/{total} = {correct / (double)total:P1}");
                }
            }
            finally
            {
                btnTest.Enabled = true;
            }
        }

        private void BtnClearCache_Click(object? sender, EventArgs e)
        {
            if (Directory.Exists(ModelCacheDir))
            {
                var files = Directory.GetFiles(ModelCacheDir, "model_cache_*.json");
                foreach (var f in files)
                {
                    File.Delete(f);
                }
                _model = null;
                Log($"Cache cleared ({files.Length} files).");
                btnTest.Enabled = false;
            }
        }

        #endregion

        #region Test Tab

        private async void BtnEvaluate_Click(object? sender, EventArgs e)
        {
            if (_model == null)
            {
                MessageBox.Show("Train a model first or load from cache.", "No Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Load dataset if model was loaded from cache
            if (_dataset == null)
            {
                txtMetrics.Text = "Loading test dataset...";
                _dataset = await Task.Run(() => DataLoader.Load(TrainCsvPath, TestCsvPath, valSplit: 0.1, seed: 42));
            }

            btnEvaluate.Enabled = false;
            txtMetrics.Clear();
            dataGridConfusion.Rows.Clear();
            dataGridConfusion.Columns.Clear();

            try
            {
                Log("Evaluating on test set...");

                // Run evaluation on background thread
                var (acc, cm) = await Task.Run(() =>
                {
                    double a = _model.EvaluateAccuracy(_dataset.TestX, _dataset.TestLabels);
                    var c = _model.ConfusionMatrix(_dataset.TestX, _dataset.TestLabels);
                    return (a, c);
                });

                // Compute per-class metrics
                var sb = new StringBuilder();
                sb.AppendLine($"Test Accuracy: {acc:P2}");
                sb.AppendLine($"Test Samples: {_dataset.TestLabels.Length}");
                sb.AppendLine();

                sb.AppendLine("Per-Class Metrics:");
                sb.AppendLine("Class\tPrecision\tRecall\t\tF1\t\tSupport");

                double macroPrec = 0, macroRec = 0, macroF1 = 0;
                int classesWithSupport = 0;

                for (int i = 0; i < _dataset.NumClasses; i++)
                {
                    int tp = cm[i, i];
                    int fp = 0, fn = 0;
                    for (int j = 0; j < _dataset.NumClasses; j++)
                    {
                        if (j != i) fp += cm[j, i];
                        if (j != i) fn += cm[i, j];
                    }
                    int support = tp + fn;

                    if (support == 0) continue;

                    double prec = tp / (double)(tp + fp);
                    double rec = tp / (double)(tp + fn);
                    double f1 = (prec + rec > 0) ? 2 * prec * rec / (prec + rec) : 0;

                    macroPrec += prec;
                    macroRec += rec;
                    macroF1 += f1;
                    classesWithSupport++;

                    sb.AppendLine($"{IndexToLabel(i)}\t{prec:P2}\t\t{rec:P2}\t\t{f1:P2}\t\t{support}");
                }

                if (classesWithSupport > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"Macro Avg: Precision={macroPrec/classesWithSupport:P2}, Recall={macroRec/classesWithSupport:P2}, F1={macroF1/classesWithSupport:P2}");
                }

                txtMetrics.Text = sb.ToString();

                // Populate confusion matrix grid
                dataGridConfusion.Columns.Add("Class", "True\\Pred");
                for (int j = 0; j < _dataset.NumClasses; j++)
                    dataGridConfusion.Columns.Add(IndexToLabel(j), IndexToLabel(j));

                for (int i = 0; i < _dataset.NumClasses; i++)
                {
                    var row = new object[_dataset.NumClasses + 1];
                    row[0] = IndexToLabel(i);
                    for (int j = 0; j < _dataset.NumClasses; j++)
                        row[j + 1] = cm[i, j];
                    dataGridConfusion.Rows.Add(row);
                }

                // Highlight diagonal
                for (int i = 0; i < _dataset.NumClasses; i++)
                    dataGridConfusion.Rows[i].Cells[i + 1].Style.BackColor = Color.LightGreen;

                Log("Evaluation complete.");
            }
            catch (Exception ex)
            {
                Log($"Evaluation error: {ex.Message}");
            }
            finally
            {
                btnEvaluate.Enabled = true;
            }
        }

        #endregion

        #region Helpers

        private void Log(string msg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(msg)));
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
            txtLog.ScrollToCaret();
        }

        private void UpdateProgress(int current, int total)
        {
            if (progressBarTraining.InvokeRequired)
            {
                progressBarTraining.Invoke(new Action(() => UpdateProgress(current, total)));
                return;
            }
            progressBarTraining.Maximum = total;
            progressBarTraining.Value = SysMath.Min(current, total);
        }

        private static string IndexToLabel(int idx)
        {
            return idx.ToString();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _trainingCts?.Cancel();
        }

        #endregion
    }
}