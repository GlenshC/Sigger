namespace Sigger.NeuralNetwork.Activations
{
    using MatrixOps = Sigger.Math.MatrixOps;

    /// <summary>ReLU: f(x) = max(0, x). Derivative: 1 if x > 0 else 0.</summary>
    public class ReLU : IActivation
    {
        public void Forward(double[,] x) => MatrixOps.MapInPlace(x, v => v > 0 ? v : 0);

        public double[,] Backward(double[,] input, double[,] gradOutput)
        {
            int rows = input.GetLength(0), cols = input.GetLength(1);
            var gradInput = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    gradInput[i, j] = input[i, j] > 0 ? gradOutput[i, j] : 0;
            return gradInput;
        }
    }
}