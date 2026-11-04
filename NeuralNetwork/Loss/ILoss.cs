namespace Sigger.NeuralNetwork.Loss
{
    /// <summary>Loss function interface.</summary>
    public interface ILoss
    {
        /// <summary>Compute loss value.</summary>
        double Forward(double[,] predictions, double[,] targets);

        /// <summary>Compute gradient of loss w.r.t predictions (dL/dy).</summary>
        double[,] Backward(double[,] predictions, double[,] targets);
    }
}