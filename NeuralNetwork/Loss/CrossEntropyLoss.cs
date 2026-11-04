namespace Sigger.NeuralNetwork.Loss
{
    using SysMath = System.Math;

    /// <summary>
    /// Cross-entropy loss for multi-class classification.
    /// When combined with Softmax output, gradient simplifies to: dL/dy = y_pred - y_true
    /// </summary>
    public class CrossEntropyLoss : ILoss
    {
        private const double Eps = 1e-15;

        public double Forward(double[,] predictions, double[,] targets)
        {
            int batch = predictions.GetLength(0), classes = predictions.GetLength(1);
            double loss = 0;

            for (int i = 0; i < batch; i++)
            {
                for (int j = 0; j < classes; j++)
                {
                    double p = SysMath.Max(Eps, SysMath.Min(1 - Eps, predictions[i, j]));
                    if (targets[i, j] > 0)
                        loss -= SysMath.Log(p);
                }
            }
            return loss / batch;
        }

        /// <summary>
        /// Gradient for Softmax + CrossEntropy: dL/dy = y_pred - y_true
        /// This is the efficient combined backward pass.
        /// </summary>
        public double[,] Backward(double[,] predictions, double[,] targets)
        {
            int batch = predictions.GetLength(0), classes = predictions.GetLength(1);
            var grad = new double[batch, classes];

            for (int i = 0; i < batch; i++)
                for (int j = 0; j < classes; j++)
                    grad[i, j] = (predictions[i, j] - targets[i, j]) / batch;

            return grad;
        }
    }
}