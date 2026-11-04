namespace Sigger.Data
{
    /// <summary>Container for dataset splits.</summary>
    public class Dataset
    {
        public double[,] TrainX { get; set; } = new double[0, 0];
        public double[,] TrainY { get; set; } = new double[0, 0];
        public double[,] ValX { get; set; } = new double[0, 0];
        public double[,] ValY { get; set; } = new double[0, 0];
        public double[,] TestX { get; set; } = new double[0, 0];
        public double[,] TestY { get; set; } = new double[0, 0];
        public int[] TrainLabels { get; set; } = Array.Empty<int>();
        public int[] ValLabels { get; set; } = Array.Empty<int>();
        public int[] TestLabels { get; set; } = Array.Empty<int>();
        public int NumClasses { get; set; } = 24;
        public int InputSize { get; set; } = 784;
    }
}