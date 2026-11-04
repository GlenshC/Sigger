using Sigger.Math;
using MatrixOps = Sigger.Math.MatrixOps;
using System.Threading.Tasks;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Sigger.NeuralNetwork.Layers
{
    /// <summary>
    /// Fully connected (dense) layer: y = xW^T + b.
    /// Input: (batch, inFeatures), Weights: (outFeatures, inFeatures), Bias: (outFeatures)
    /// Output: (batch, outFeatures)
    /// </summary>
    public class DenseLayer : ILayer
    {
        private double[,] _weights;      // (outFeatures, inFeatures)
        private double[] _biases;        // (outFeatures)
        internal double[,] _weightGrads;  // Same shape as weights (internal for gradient accumulation)
        internal double[] _biasGrads;     // Same shape as biases (internal for gradient accumulation)
        private double[,] _lastInput;    // Cached for backward
        private double[,] _weightsT;     // Pre-computed weights transpose
        private double[,] _gradOutputT;  // Reusable transpose buffer

        public int InFeatures { get; }
        public int OutFeatures { get; }
        public int ParameterCount => InFeatures * OutFeatures + OutFeatures;

        public DenseLayer(int inFeatures, int outFeatures, Random? rng = null)
        {
            InFeatures = inFeatures;
            OutFeatures = outFeatures;
            _weights = MatrixOps.Random(outFeatures, inFeatures, scale: 1.0, rng);
            _biases = new double[outFeatures];
            _weightGrads = new double[outFeatures, inFeatures];
            _biasGrads = new double[outFeatures];
            _weightsT = MatrixOps.Transpose(_weights); // Pre-compute transpose
            _gradOutputT = new double[outFeatures, 1]; // Will resize as needed
        }

        public double[,] Forward(double[,] input)
        {
            _lastInput = MatrixOps.Copy(input); // Cache for backward
            // output = input * weights^T + biases (using pre-computed transpose)
            var output = MatrixOps.MultiplyParallel(input, _weightsT);
            // Add biases to each row (SIMD optimized)
            int batch = output.GetLength(0);
            MatrixOps.AddBiasToRows(output, _biases);
            return output;
        }

        public double[,] Backward(double[,] gradOutput)
        {
            int batch = gradOutput.GetLength(0);

            // Resize transpose buffer if needed
            if (_gradOutputT.GetLength(1) != batch)
            {
                _gradOutputT = new double[OutFeatures, batch];
            }

            // gradOutputT = gradOutput^T  (outFeatures, batch)
            TransposeInPlace(gradOutput, _gradOutputT);

            // gradWeights = gradOutput^T * input  (outFeatures, inFeatures)
            MatrixOps.Multiply(_gradOutputT, _lastInput, _weightGrads);

            // gradBiases = sum over batch of gradOutput
            _biasGrads = MatrixOps.SumRows(gradOutput);

            // gradInput = gradOutput * weights  (batch, inFeatures)
            var gradInput = MatrixOps.MultiplyParallel(gradOutput, _weights);

            return gradInput;
        }

        private static void TransposeInPlace(double[,] src, double[,] dst)
        {
            int rows = src.GetLength(0), cols = src.GetLength(1);
            // dst is (cols, rows)
            Parallel.For(0, rows, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                for (int j = 0; j < cols; j++)
                    dst[j, i] = src[i, j];
            });
        }

        public void Update(double learningRate)
        {
            // weights -= lr * gradWeights
            MatrixOps.MultiplyScalarInPlace(_weightGrads, -learningRate);
            MatrixOps.AddInPlace(_weights, _weightGrads);

            // Update pre-computed transpose
            _weightsT = MatrixOps.Transpose(_weights);

            // biases -= lr * gradBiases (SIMD optimized)
            MatrixOps.SubtractScalarInPlace(_biases, learningRate, _biasGrads);
        }

        public (double[,] weights, double[] biases) GetParameters()
            => (MatrixOps.Copy(_weights), (double[])_biases.Clone());

        public void SetParameters(double[,] weights, double[] biases)
        {
            _weights = MatrixOps.Copy(weights);
            _biases = (double[])biases.Clone();
            _weightsT = MatrixOps.Transpose(_weights);
        }
    }
}