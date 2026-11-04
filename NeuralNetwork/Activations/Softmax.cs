namespace Sigger.NeuralNetwork.Activations
{
    using SysMath = System.Math;

    /// <summary>
    /// Softmax: f(x)_i = exp(x_i) / sum(exp(x_j)).
    /// Numerically stable: subtract max before exp.
    /// Derivative handled combined with CrossEntropyLoss for efficiency.
    /// </summary>
    public class Softmax : IActivation
    {
        public void Forward(double[,] x)
        {
            int rows = x.GetLength(0), cols = x.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                // Find max for numerical stability
                double max = x[i, 0];
                for (int j = 1; j < cols; j++)
                    if (x[i, j] > max) max = x[i, j];

                // Exp and sum
                double sum = 0;
                for (int j = 0; j < cols; j++)
                {
                    double v = SysMath.Exp(x[i, j] - max);
                    x[i, j] = v;
                    sum += v;
                }

                // Normalize
                for (int j = 0; j < cols; j++)
                    x[i, j] /= sum;
            }
        }

        /// <summary>
        /// Softmax backward alone is complex (Jacobian).
        /// Normally combined with CrossEntropy: grad = output - target.
        /// This standalone version computes full Jacobian (slow, for completeness).
        /// </summary>
        public double[,] Backward(double[,] input, double[,] gradOutput)
        {
            int rows = input.GetLength(0), cols = input.GetLength(1);
            var gradInput = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                // Reconstruct output from input (softmax already applied in-place)
                double[] y = new double[cols];
                for (int j = 0; j < cols; j++) y[j] = input[i, j];

                for (int j = 0; j < cols; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < cols; k++)
                    {
                        double dYj_dXk = (j == k) ? y[j] * (1 - y[j]) : -y[j] * y[k];
                        sum += dYj_dXk * gradOutput[i, k];
                    }
                    gradInput[i, j] = sum;
                }
            }
            return gradInput;
        }
    }
}