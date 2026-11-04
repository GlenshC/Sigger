using System.Text;
using Sigger.Math;

namespace Sigger.Data
{
    /// <summary>
    /// Loads and preprocesses Digit MNIST CSV data (0-9).
    /// Handles normalization (0-1), train/val split, one-hot encoding.
    /// </summary>
    public static class DataLoader
    {
        private const int ImageSize = 28 * 28; // 784
        private const int NumClasses = 10;      // Digits 0-9

        /// <summary>
        /// Load training and test CSV, split train into train/val, normalize, one-hot encode.
        /// </summary>
        public static Dataset Load(
            string trainCsvPath,
            string testCsvPath,
            double valSplit = 0.1,
            int? seed = null)
        {
            var rng = seed.HasValue ? new Random(seed.Value) : Random.Shared;

            // Load raw data
            var (trainXRaw, trainLabelsRaw) = LoadCsv(trainCsvPath);
            var (testXRaw, testLabelsRaw) = LoadCsv(testCsvPath);

            // Normalize to [0, 1]
            NormalizeInPlace(trainXRaw);
            NormalizeInPlace(testXRaw);

            // Shuffle train
            int nTrain = trainXRaw.GetLength(0);
            var indices = Enumerable.Range(0, nTrain).ToArray();
            for (int i = nTrain - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
                (trainLabelsRaw[i], trainLabelsRaw[j]) = (trainLabelsRaw[j], trainLabelsRaw[i]);
            }

            // Apply shuffle to trainX
            var trainXShuffled = new double[nTrain, ImageSize];
            for (int i = 0; i < nTrain; i++)
                for (int j = 0; j < ImageSize; j++)
                    trainXShuffled[i, j] = trainXRaw[indices[i], j];

            // Split train/val
            int valCount = (int)(nTrain * valSplit);
            int trainCount = nTrain - valCount;

            var dataset = new Dataset
            {
                NumClasses = NumClasses,
                InputSize = ImageSize
            };

            // Train
            dataset.TrainX = new double[trainCount, ImageSize];
            dataset.TrainY = new double[trainCount, NumClasses];
            dataset.TrainLabels = new int[trainCount];
            for (int i = 0; i < trainCount; i++)
            {
                for (int j = 0; j < ImageSize; j++)
                    dataset.TrainX[i, j] = trainXShuffled[i, j];
                dataset.TrainLabels[i] = trainLabelsRaw[i];
                OneHot(dataset.TrainY, i, trainLabelsRaw[i]);
            }

            // Val
            dataset.ValX = new double[valCount, ImageSize];
            dataset.ValY = new double[valCount, NumClasses];
            dataset.ValLabels = new int[valCount];
            for (int i = 0; i < valCount; i++)
            {
                int src = trainCount + i;
                for (int j = 0; j < ImageSize; j++)
                    dataset.ValX[i, j] = trainXShuffled[src, j];
                dataset.ValLabels[i] = trainLabelsRaw[src];
                OneHot(dataset.ValY, i, trainLabelsRaw[src]);
            }

            // Test
            int nTest = testXRaw.GetLength(0);
            dataset.TestX = testXRaw;
            dataset.TestY = new double[nTest, NumClasses];
            dataset.TestLabels = testLabelsRaw;
            for (int i = 0; i < nTest; i++)
                OneHot(dataset.TestY, i, testLabelsRaw[i]);

            return dataset;
        }

        /// <summary>Load CSV: returns (features, labels). Skips header.</summary>
        private static (double[,] features, int[] labels) LoadCsv(string path)
        {
            var lines = File.ReadAllLines(path);
            if (lines.Length < 2) throw new InvalidDataException($"Empty CSV: {path}");

            int n = lines.Length - 1;
            var features = new double[n, ImageSize];
            var labels = new int[n];

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length != ImageSize + 1)
                    throw new InvalidDataException($"Row {i}: expected {ImageSize + 1} columns, got {parts.Length}");

                labels[i - 1] = int.Parse(parts[0]);

                for (int j = 0; j < ImageSize; j++)
                    features[i - 1, j] = double.Parse(parts[j + 1]);
            }

            return (features, labels);
        }

        /// <summary>Normalize pixel values from [0,255] to [0,1] in-place.</summary>
        private static void NormalizeInPlace(double[,] data)
        {
            int rows = data.GetLength(0), cols = data.GetLength(1);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    data[i, j] /= 255.0;
        }

        /// <summary>Set one-hot encoding for a single row.</summary>
        private static void OneHot(double[,] y, int row, int label)
        {
            for (int j = 0; j < y.GetLength(1); j++)
                y[row, j] = (j == label) ? 1.0 : 0.0;
        }

        /// <summary>Demo: load and print stats.</summary>
        public static void Demo(string trainPath, string testPath)
        {
            var ds = Load(trainPath, testPath);
            Console.WriteLine($"Train: {ds.TrainX.GetLength(0)} samples, {ds.TrainX.GetLength(1)} features");
            Console.WriteLine($"Val:   {ds.ValX.GetLength(0)} samples");
            Console.WriteLine($"Test:  {ds.TestX.GetLength(0)} samples");
            Console.WriteLine($"Classes: {ds.NumClasses}");
            Console.WriteLine($"Label range: train [{ds.TrainLabels.Min()}-{ds.TrainLabels.Max()}], test [{ds.TestLabels.Min()}-{ds.TestLabels.Max()}]");
        }
    }
}