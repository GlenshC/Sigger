using Sigger.NeuralNetwork.Layers;
using Sigger.NeuralNetwork.Loss;
using Sigger.NeuralNetwork.Activations;
using System.Text.Json;
using SysMath = System.Math;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Sigger.NeuralNetwork
{
    /// <summary>
    /// Multi-Layer Perceptron: Input → Hidden (ReLU) → Output (Softmax).
    /// Handles forward, backward, training loop, and model persistence.
    /// </summary>
    public class MLP
    {
        private readonly List<ILayer> _layers = new();
        private readonly ILoss _loss;
        private readonly int _inputSize;
        private readonly int _hiddenSize;
        private readonly int _outputSize;
        private readonly Random _rng;

        public int InputSize => _inputSize;
        public int HiddenSize => _hiddenSize;
        public int OutputSize => _outputSize;

        /// <summary>
        /// Create MLP: 784 → hidden → 24 (Sign Language MNIST classes).
        /// </summary>
        public MLP(int inputSize, int hiddenSize, int outputSize, int? seed = null)
        {
            _inputSize = inputSize;
            _hiddenSize = hiddenSize;
            _outputSize = outputSize;
            _rng = seed.HasValue ? new Random(seed.Value) : Random.Shared;
            _loss = new CrossEntropyLoss();

            // Input → Hidden (Dense + ReLU)
            _layers.Add(new DenseLayer(inputSize, hiddenSize, _rng));
            _layers.Add(new ActivationLayer(new ReLU()));

            // Hidden → Output (Dense + Softmax)
            _layers.Add(new DenseLayer(hiddenSize, outputSize, _rng));
            _layers.Add(new ActivationLayer(new Softmax()));
        }

        /// <summary>Forward pass through all layers.</summary>
        public double[,] Forward(double[,] input)
        {
            double[,] x = input;
            foreach (var layer in _layers)
                x = layer.Forward(x);
            return x;
        }

        /// <summary>Backward pass through all layers in reverse.</summary>
        public void Backward(double[,] gradOutput)
        {
            double[,] grad = gradOutput;
            for (int i = _layers.Count - 1; i >= 0; i--)
                grad = _layers[i].Backward(grad);
        }

        /// <summary>Single training step on a batch (forward + backward + update).</summary>
        public double TrainStep(double[,] inputs, double[,] targets, double learningRate)
        {
            var predictions = Forward(inputs);
            double loss = _loss.Forward(predictions, targets);
            var grad = _loss.Backward(predictions, targets);
            Backward(grad);

            foreach (var layer in _layers)
                layer.Update(learningRate);

            return loss;
        }

        /// <summary>Predict class indices for inputs (parallelized).</summary>
        public int[] Predict(double[,] inputs)
        {
            var probs = Forward(inputs);
            int batch = probs.GetLength(0), classes = probs.GetLength(1);
            var preds = new int[batch];

            Parallel.For(0, batch, i =>
            {
                int best = 0;
                double bestProb = probs[i, 0];
                for (int j = 1; j < classes; j++)
                    if (probs[i, j] > bestProb) { bestProb = probs[i, j]; best = j; }
                preds[i] = best;
            });
            return preds;
        }

        /// <summary>Get prediction probabilities.</summary>
        public double[,] PredictProba(double[,] inputs) => Forward(inputs);

        /// <summary>Evaluate accuracy on dataset (parallelized).</summary>
        public double EvaluateAccuracy(double[,] inputs, int[] targets)
        {
            var preds = Predict(inputs);
            int correct = 0;
            Parallel.For(0, targets.Length, i =>
            {
                if (preds[i] == targets[i]) Interlocked.Increment(ref correct);
            });
            return (double)correct / targets.Length;
        }

        /// <summary>Get confusion matrix (parallelized).</summary>
        public int[,] ConfusionMatrix(double[,] inputs, int[] targets)
        {
            var preds = Predict(inputs);
            var cm = new int[_outputSize, _outputSize];

            Parallel.For(0, targets.Length, i =>
            {
                Interlocked.Increment(ref cm[targets[i], preds[i]]);
            });
            return cm;
        }

        /// <summary>Train for multiple epochs with mini-batches (parallelized matrix ops per batch).</summary>
        public List<double> Train(
            double[,] trainX, double[,] trainY,
            int epochs, int batchSize, double learningRate,
            double[,]? valX = null, double[,]? valY = null,
            Action<int, double, double?>? onEpochEnd = null,
            int earlyStopPatience = 10,
            CancellationToken cancellationToken = default)
        {
            int n = trainX.GetLength(0);
            var losses = new List<double>();
            double bestValLoss = double.MaxValue;
            int noImprove = 0;
            var indices = Enumerable.Range(0, n).ToArray();

            int batchesPerEpoch = (n + batchSize - 1) / batchSize;

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                // Shuffle
                for (int i = n - 1; i > 0; i--)
                {
                    int j = _rng.Next(i + 1);
                    (indices[i], indices[j]) = (indices[j], indices[i]);
                }

                double epochLoss = 0;
                int batches = 0;

                // Process batches sequentially (correct gradient updates)
                // Matrix operations inside each batch are parallelized
                for (int batchIdx = 0; batchIdx < batchesPerEpoch; batchIdx++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    int start = batchIdx * batchSize;
                    int end = SysMath.Min(start + batchSize, n);
                    int batchCount = end - start;

                    var batchX = new double[batchCount, _inputSize];
                    var batchY = new double[batchCount, _outputSize];
                    for (int b = 0; b < batchCount; b++)
                    {
                        int idx = indices[start + b];
                        for (int j = 0; j < _inputSize; j++) batchX[b, j] = trainX[idx, j];
                        for (int j = 0; j < _outputSize; j++) batchY[b, j] = trainY[idx, j];
                    }

                    epochLoss += TrainStep(batchX, batchY, learningRate);
                    batches++;
                }

                double avgLoss = batches > 0 ? epochLoss / batches : 0;
                losses.Add(avgLoss);

                double? valLoss = null;
                if (valX != null && valY != null)
                {
                    var valPreds = Forward(valX);
                    valLoss = _loss.Forward(valPreds, valY);

                    if (valLoss < bestValLoss)
                    {
                        bestValLoss = valLoss.Value;
                        noImprove = 0;
                    }
                    else
                    {
                        noImprove++;
                        if (noImprove >= earlyStopPatience && earlyStopPatience > 0)
                        {
                            onEpochEnd?.Invoke(epoch, avgLoss, valLoss);
                            break;
                        }
                    }
                }

                onEpochEnd?.Invoke(epoch, avgLoss, valLoss);
            }

            return losses;
        }

        /// <summary>Save model weights to JSON file.</summary>
        public void Save(string path)
        {
            var data = new ModelData
            {
                InputSize = _inputSize,
                HiddenSize = _hiddenSize,
                OutputSize = _outputSize,
                Layers = new List<LayerData>()
            };

            foreach (var layer in _layers)
            {
                var (w, b) = layer.GetParameters();
                data.Layers.Add(new LayerData
                {
                    Weights = Flatten(w),
                    Biases = b,
                    WeightShape = new[] { w.GetLength(0), w.GetLength(1) }
                });
            }

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        /// <summary>Load model weights from JSON file.</summary>
        public static MLP Load(string path)
        {
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<ModelData>(json)
                ?? throw new InvalidDataException("Invalid model file");

            var mlp = new MLP(data.InputSize, data.HiddenSize, data.OutputSize);

            for (int i = 0; i < mlp._layers.Count; i++)
            {
                var ld = data.Layers[i];
                var w = Reshape(ld.Weights, ld.WeightShape[0], ld.WeightShape[1]);
                mlp._layers[i].SetParameters(w, ld.Biases);
            }

            return mlp;
        }

        private static double[] Flatten(double[,] m)
        {
            var arr = new double[m.Length];
            Buffer.BlockCopy(m, 0, arr, 0, m.Length * sizeof(double));
            return arr;
        }

        private static double[,] Reshape(double[] flat, int rows, int cols)
        {
            var m = new double[rows, cols];
            Buffer.BlockCopy(flat, 0, m, 0, flat.Length * sizeof(double));
            return m;
        }

        // Serialization DTOs
        private class ModelData
        {
            public int InputSize { get; set; }
            public int HiddenSize { get; set; }
            public int OutputSize { get; set; }
            public List<LayerData> Layers { get; set; } = new();
        }

        private class LayerData
        {
            public double[] Weights { get; set; } = Array.Empty<double>();
            public double[] Biases { get; set; } = Array.Empty<double>();
            public int[] WeightShape { get; set; } = Array.Empty<int>();
        }
    }
}