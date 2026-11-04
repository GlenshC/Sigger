using Sigger.NeuralNetwork.Activations;
using MatrixOps = Sigger.Math.MatrixOps;

namespace Sigger.NeuralNetwork.Layers
{
    /// <summary>Layer that applies an activation function element-wise.</summary>
    public class ActivationLayer : ILayer
    {
        private readonly IActivation _activation;
        private double[,]? _lastInput;

        public int ParameterCount => 0;

        public ActivationLayer(IActivation activation)
        {
            _activation = activation;
        }

        public double[,] Forward(double[,] input)
        {
            _lastInput = MatrixOps.Copy(input);
            _activation.Forward(_lastInput); // In-place
            return _lastInput;
        }

        public double[,] Backward(double[,] gradOutput)
        {
            return _activation.Backward(_lastInput, gradOutput);
        }

        public void Update(double learningRate) { } // No parameters

        public (double[,] weights, double[] biases) GetParameters()
            => (new double[0, 0], new double[0]);

        public void SetParameters(double[,] weights, double[] biases) { }
    }
}