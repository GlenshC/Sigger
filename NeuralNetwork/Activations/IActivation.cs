namespace Sigger.NeuralNetwork.Activations
{
    /// <summary>Activation function interface: forward + derivative.</summary>
    public interface IActivation
    {
        /// <summary>Forward pass: apply activation element-wise.</summary>
        void Forward(double[,] x);

        /// <summary>Backward pass: compute gradient given upstream gradient and cached input.</summary>
        /// <param name="input">Cached input from forward pass</param>
        /// <param name="gradOutput">Upstream gradient (dL/dy)</param>
        /// <returns>Gradient w.r.t input (dL/dx)</returns>
        double[,] Backward(double[,] input, double[,] gradOutput);
    }
}