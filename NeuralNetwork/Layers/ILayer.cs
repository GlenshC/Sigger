namespace Sigger.NeuralNetwork.Layers
{
    /// <summary>Layer interface: forward + backward + parameter updates.</summary>
    public interface ILayer
    {
        /// <summary>Forward pass.</summary>
        double[,] Forward(double[,] input);

        /// <summary>Backward pass: compute gradients and return gradient w.r.t input.</summary>
        /// <param name="gradOutput">Upstream gradient (dL/dy)</param>
        /// <returns>Gradient w.r.t input (dL/dx)</returns>
        double[,] Backward(double[,] gradOutput);

        /// <summary>Update parameters with gradients (SGD).</summary>
        void Update(double learningRate);

        /// <summary>Get all parameters as flat arrays for serialization.</summary>
        (double[,] weights, double[] biases) GetParameters();

        /// <summary>Set parameters from flat arrays (for loading).</summary>
        void SetParameters(double[,] weights, double[] biases);

        /// <summary>Total parameter count.</summary>
        int ParameterCount { get; }
    }
}