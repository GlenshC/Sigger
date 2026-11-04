namespace Sigger.Math
{
    using SysMath = System.Math;
    using System.Runtime.Intrinsics;
    using System.Runtime.Intrinsics.X86;

    /// <summary>
    /// Simple 2D matrix operations for neural network computations.
    /// Uses double[,] for cache-friendly row-major layout.
    /// Optimized with AVX2/SIMD where available.
    /// </summary>
    public static class MatrixOps
    {
        /// <summary>Create matrix filled with value.</summary>
        public static double[,] Create(int rows, int cols, double value = 0.0)
        {
            var m = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    m[i, j] = value;
            return m;
        }

        /// <summary>Create matrix from jagged array.</summary>
        public static double[,] FromJagged(double[][] jagged)
        {
            int rows = jagged.Length;
            int cols = jagged[0].Length;
            var m = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    m[i, j] = jagged[i][j];
            return m;
        }

        /// <summary>Create random matrix with Xavier/Glorot initialization.</summary>
        public static double[,] Random(int rows, int cols, double scale = 1.0, Random? rng = null)
        {
            rng ??= new Random();
            var m = new double[rows, cols];
            double limit = SysMath.Sqrt(6.0 / (rows + cols)) * scale;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    m[i, j] = rng.NextDouble() * 2 * limit - limit;
            return m;
        }

        /// <summary>Matrix multiply: A (m×n) × B (n×p) = C (m×p) with parallelism and SIMD.</summary>
        public static double[,] MultiplyParallel(double[,] a, double[,] b)
        {
            int m = a.GetLength(0), n = a.GetLength(1);
            int n2 = b.GetLength(0), p = b.GetLength(1);
            if (n != n2) throw new ArgumentException($"Dimension mismatch: {n} != {n2}");

            var c = new double[m, p];

            // Use larger parallel chunk size to reduce overhead for small matrices
            // Block size of 16-32 rows per task is typically optimal
            int blockSize = SysMath.Max(16, m / (Environment.ProcessorCount * 4));

            Parallel.For(0, m, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, () => 0, (i, loopState, local) =>
            {
                var rowA = GetRowSpan(a, i);
                var rowC = GetRowSpan(c, i);
                for (int k = 0; k < n; k++)
                {
                    double aik = rowA[k];
                    var rowB = GetRowSpan(b, k);
                    if (Avx2.IsSupported && p >= 4)
                    {
                        MultiplyAddAvx2(rowC, rowB, aik, p);
                    }
                    else
                    {
                        for (int j = 0; j < p; j++)
                            rowC[j] += aik * rowB[j];
                    }
                }
                return local;
            }, _ => { });

            return c;
        }

        /// <summary>Matrix multiply into pre-allocated output with parallelism and SIMD.</summary>
        public static void MultiplyParallel(double[,] a, double[,] b, double[,] output)
        {
            int m = a.GetLength(0), n = a.GetLength(1);
            int n2 = b.GetLength(0), p = b.GetLength(1);
            if (n != n2) throw new ArgumentException($"Dimension mismatch: {n} != {n2}");
            if (output.GetLength(0) != m || output.GetLength(1) != p) throw new ArgumentException("Output dimension mismatch");

            Array.Clear(output, 0, output.Length);

            Parallel.For(0, m, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                var rowA = GetRowSpan(a, i);
                var rowOut = GetRowSpan(output, i);
                for (int k = 0; k < n; k++)
                {
                    double aik = rowA[k];
                    var rowB = GetRowSpan(b, k);
                    if (Avx2.IsSupported && p >= 4)
                    {
                        MultiplyAddAvx2(rowOut, rowB, aik, p);
                    }
                    else
                    {
                        for (int j = 0; j < p; j++)
                            rowOut[j] += aik * rowB[j];
                    }
                }
            });
        }

        /// <summary>SIMD multiply-add: out += a * scalar (AVX2, 4 doubles at a time).</summary>
        private static unsafe void MultiplyAddAvx2(Span<double> output, Span<double> input, double scalar, int length)
        {
            fixed (double* outPtr = &output[0])
            fixed (double* inPtr = &input[0])
            {
                Vector256<double> vScalar = Vector256.Create(scalar);
                int j = 0;
                for (; j <= length - 4; j += 4)
                {
                    Vector256<double> vIn = Avx2.LoadVector256(inPtr + j);
                    Vector256<double> vOut = Avx2.LoadVector256(outPtr + j);
                    Vector256<double> vMul = Avx2.Multiply(vIn, vScalar);
                    Vector256<double> vAdd = Avx2.Add(vOut, vMul);
                    Avx2.Store(outPtr + j, vAdd);
                }
                for (; j < length; j++)
                    outPtr[j] += inPtr[j] * scalar;
            }
        }

        /// <summary>Matrix multiply: A (m×n) × B (n×p) = C (m×p) with SIMD.</summary>
        public static double[,] Multiply(double[,] a, double[,] b)
        {
            int m = a.GetLength(0), n = a.GetLength(1);
            int n2 = b.GetLength(0), p = b.GetLength(1);
            if (n != n2) throw new ArgumentException($"Dimension mismatch: {n} != {n2}");

            var c = new double[m, p];
            for (int i = 0; i < m; i++)
            {
                var rowA = GetRowSpan(a, i);
                var rowC = GetRowSpan(c, i);
                for (int k = 0; k < n; k++)
                {
                    double aik = rowA[k];
                    var rowB = GetRowSpan(b, k);
                    if (Avx2.IsSupported && p >= 4)
                    {
                        MultiplyAddAvx2(rowC, rowB, aik, p);
                    }
                    else
                    {
                        for (int j = 0; j < p; j++)
                            rowC[j] += aik * rowB[j];
                    }
                }
            }
            return c;
        }

        /// <summary>Matrix multiply into pre-allocated output (A × B^T = C).</summary>
        public static void MultiplyTransposeB(double[,] a, double[,] b, double[,] output)
        {
            int m = a.GetLength(0), n = a.GetLength(1);
            int p = b.GetLength(0); // b is (p, n) - we want A × B^T
            if (b.GetLength(1) != n) throw new ArgumentException($"Dimension mismatch: {b.GetLength(1)} != {n}");
            if (output.GetLength(0) != m || output.GetLength(1) != p) throw new ArgumentException("Output dimension mismatch");

            // Clear output
            Array.Clear(output, 0, output.Length);

            for (int i = 0; i < m; i++)
            {
                for (int k = 0; k < n; k++)
                {
                    double aik = a[i, k];
                    for (int j = 0; j < p; j++)
                        output[i, j] += aik * b[j, k]; // b[j, k] = B^T[k, j]
                }
            }
        }

        /// <summary>Matrix multiply: A × B^T into pre-allocated output.</summary>
        public static void MultiplyATB(double[,] a, double[,] b, double[,] output)
        {
            int m = a.GetLength(0), n = a.GetLength(1);
            int p = b.GetLength(0);
            if (b.GetLength(1) != n) throw new ArgumentException($"Dimension mismatch: {b.GetLength(1)} != {n}");
            if (output.GetLength(0) != m || output.GetLength(1) != p) throw new ArgumentException("Output dimension mismatch");

            Array.Clear(output, 0, output.Length);

            for (int i = 0; i < m; i++)
            {
                for (int k = 0; k < n; k++)
                {
                    double aik = a[i, k];
                    for (int j = 0; j < p; j++)
                        output[i, j] += aik * b[j, k];
                }
            }
        }

        /// <summary>Matrix multiply: A (m×n) × B (n×p) = C (m×p) into pre-allocated output with SIMD.</summary>
        public static void Multiply(double[,] a, double[,] b, double[,] output)
        {
            int m = a.GetLength(0), n = a.GetLength(1);
            int n2 = b.GetLength(0), p = b.GetLength(1);
            if (n != n2) throw new ArgumentException($"Dimension mismatch: {n} != {n2}");
            if (output.GetLength(0) != m || output.GetLength(1) != p) throw new ArgumentException("Output dimension mismatch");

            Array.Clear(output, 0, output.Length);

            if (Avx2.IsSupported && p >= 4)
            {
                Parallel.For(0, m, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    var rowA = GetRowSpan(a, i);
                    var rowOut = GetRowSpan(output, i);
                    for (int k = 0; k < n; k++)
                    {
                        double aik = rowA[k];
                        var rowB = GetRowSpan(b, k);
                        MultiplyAddAvx2(rowOut, rowB, aik, p);
                    }
                });
            }
            else
            {
                Parallel.For(0, m, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    for (int k = 0; k < n; k++)
                    {
                        double aik = a[i, k];
                        for (int j = 0; j < p; j++)
                            output[i, j] += aik * b[k, j];
                    }
                });
            }
        }

        /// <summary>Matrix-vector multiply: A (m×n) × v (n) = out (m).</summary>
        public static double[] Multiply(double[,] a, double[] v)
        {
            int m = a.GetLength(0), n = a.GetLength(1);
            if (n != v.Length) throw new ArgumentException($"Dimension mismatch: {n} != {v.Length}");

            var output = new double[m];
            for (int i = 0; i < m; i++)
            {
                double sum = 0;
                for (int j = 0; j < n; j++)
                    sum += a[i, j] * v[j];
                output[i] = sum;
            }
            return output;
        }

        /// <summary>Transpose matrix.</summary>
        public static double[,] Transpose(double[,] a)
        {
            int rows = a.GetLength(0), cols = a.GetLength(1);
            var t = new double[cols, rows];
            Parallel.For(0, rows, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                for (int j = 0; j < cols; j++)
                    t[j, i] = a[i, j];
            });
            return t;
        }

        /// <summary>Add matrix b to a (in-place): a += b.</summary>
        public static void AddInPlace(double[,] a, double[,] b)
        {
            int rows = a.GetLength(0), cols = a.GetLength(1);
            Parallel.For(0, rows, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                for (int j = 0; j < cols; j++)
                    a[i, j] += b[i, j];
            });
        }

        /// <summary>Add scalar to matrix (in-place) with SIMD.</summary>
        public static void AddScalarInPlace(double[,] a, double scalar)
        {
            int rows = a.GetLength(0), cols = a.GetLength(1);
            if (Avx2.IsSupported)
            {
                Parallel.For(0, rows, i =>
                {
                    var row = GetRowSpan(a, i);
                    AddScalarAvx2(row, scalar);
                });
            }
            else
            {
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        a[i, j] += scalar;
            }
        }

        private static unsafe void AddScalarAvx2(Span<double> row, double scalar)
        {
            fixed (double* ptr = &row[0])
            {
                Vector256<double> vScalar = Vector256.Create(scalar);
                int len = row.Length;
                int j = 0;
                for (; j <= len - 4; j += 4)
                {
                    Vector256<double> v = Avx2.LoadVector256(ptr + j);
                    Avx2.Store(ptr + j, Avx2.Add(v, vScalar));
                }
                for (; j < len; j++)
                    ptr[j] += scalar;
            }
        }

        /// <summary>Multiply matrix by scalar (in-place) with SIMD.</summary>
        public static void MultiplyScalarInPlace(double[,] a, double scalar)
        {
            int rows = a.GetLength(0), cols = a.GetLength(1);
            if (Avx2.IsSupported)
            {
                Parallel.For(0, rows, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    var row = GetRowSpan(a, i);
                    MultiplyScalarAvx2(row, scalar);
                });
            }
            else
            {
                Parallel.For(0, rows, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    for (int j = 0; j < cols; j++)
                        a[i, j] *= scalar;
                });
            }
        }

        private static unsafe void MultiplyScalarAvx2(Span<double> row, double scalar)
        {
            fixed (double* ptr = &row[0])
            {
                Vector256<double> vScalar = Vector256.Create(scalar);
                int len = row.Length;
                int j = 0;
                for (; j <= len - 4; j += 4)
                {
                    Vector256<double> v = Avx2.LoadVector256(ptr + j);
                    Avx2.Store(ptr + j, Avx2.Multiply(v, vScalar));
                }
                for (; j < len; j++)
                    ptr[j] *= scalar;
            }
        }

        /// <summary>Add bias vector to each row of matrix (in-place) with SIMD: row += bias.</summary>
        public static void AddBiasToRows(double[,] matrix, double[] bias)
        {
            int rows = matrix.GetLength(0), cols = matrix.GetLength(1);
            if (bias.Length != cols) throw new ArgumentException($"Bias length {bias.Length} != cols {cols}");

            if (Avx2.IsSupported && cols >= 4)
            {
                Parallel.For(0, rows, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    var row = GetRowSpan(matrix, i);
                    AddBiasAvx2(row, bias);
                });
            }
            else
            {
                Parallel.For(0, rows, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    for (int j = 0; j < cols; j++)
                        matrix[i, j] += bias[j];
                });
            }
        }

        private static unsafe void AddBiasAvx2(Span<double> row, double[] bias)
        {
            fixed (double* rowPtr = &row[0])
            fixed (double* biasPtr = &bias[0])
            {
                int len = row.Length;
                int j = 0;
                for (; j <= len - 4; j += 4)
                {
                    Vector256<double> vRow = Avx2.LoadVector256(rowPtr + j);
                    Vector256<double> vBias = Avx2.LoadVector256(biasPtr + j);
                    Avx2.Store(rowPtr + j, Avx2.Add(vRow, vBias));
                }
                for (; j < len; j++)
                    rowPtr[j] += biasPtr[j];
            }
        }

        /// <summary>Subtract lr * grad from biases (in-place): bias -= lr * grad with SIMD.</summary>
        public static void SubtractScalarInPlace(double[] bias, double lr, double[] grad)
        {
            int len = bias.Length;
            if (grad.Length != len) throw new ArgumentException("grad length mismatch");

            if (Avx2.IsSupported && len >= 4)
            {
                unsafe
                {
                    fixed (double* biasPtr = &bias[0])
                    fixed (double* gradPtr = &grad[0])
                    {
                        Vector256<double> vLr = Vector256.Create(lr);
                        int j = 0;
                        for (; j <= len - 4; j += 4)
                        {
                            Vector256<double> vBias = Avx2.LoadVector256(biasPtr + j);
                            Vector256<double> vGrad = Avx2.LoadVector256(gradPtr + j);
                            Vector256<double> vSub = Avx2.Multiply(vGrad, vLr);
                            Avx2.Store(biasPtr + j, Avx2.Subtract(vBias, vSub));
                        }
                        for (; j < len; j++)
                            biasPtr[j] -= lr * gradPtr[j];
                    }
                }
            }
            else
            {
                for (int j = 0; j < len; j++)
                    bias[j] -= lr * grad[j];
            }
        }

        /// <summary>Element-wise multiply (Hadamard product): a ⊙ b (in-place on a) with SIMD.</summary>
        public static void HadamardInPlace(double[,] a, double[,] b)
        {
            int rows = a.GetLength(0), cols = a.GetLength(1);
            if (Avx2.IsSupported)
            {
                Parallel.For(0, rows, i =>
                {
                    var rowA = GetRowSpan(a, i);
                    var rowB = GetRowSpan(b, i);
                    HadamardAvx2(rowA, rowB);
                });
            }
            else
            {
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        a[i, j] *= b[i, j];
            }
        }

        private static unsafe void HadamardAvx2(Span<double> a, Span<double> b)
        {
            fixed (double* aPtr = &a[0])
            fixed (double* bPtr = &b[0])
            {
                int len = a.Length;
                int j = 0;
                for (; j <= len - 4; j += 4)
                {
                    Vector256<double> vA = Avx2.LoadVector256(aPtr + j);
                    Vector256<double> vB = Avx2.LoadVector256(bPtr + j);
                    Avx2.Store(aPtr + j, Avx2.Multiply(vA, vB));
                }
                for (; j < len; j++)
                    aPtr[j] *= bPtr[j];
            }
        }

        /// <summary>Element-wise apply function (in-place).</summary>
        public static void MapInPlace(double[,] a, Func<double, double> f)
        {
            int rows = a.GetLength(0), cols = a.GetLength(1);
            Parallel.For(0, rows, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                for (int j = 0; j < cols; j++)
                    a[i, j] = f(a[i, j]);
            });
        }

        /// <summary>Row-wise sum: sum each column across rows → vector of length cols.</summary>
        public static double[] SumRows(double[,] a)
        {
            int rows = a.GetLength(0), cols = a.GetLength(1);
            var sums = new double[cols];
            if (Avx2.IsSupported)
            {
                var localSums = new double[Environment.ProcessorCount, cols];
                Parallel.For(0, rows, i =>
                {
                    int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId % Environment.ProcessorCount;
                    var row = GetRowSpan(a, i);
                    var local = localSums;
                    unsafe
                    {
                        fixed (double* sPtr = &local[threadId, 0])
                        fixed (double* rPtr = &row[0])
                        {
                            int j = 0;
                            for (; j <= cols - 4; j += 4)
                            {
                                Vector256<double> vSum = Avx2.LoadVector256(sPtr + j);
                                Vector256<double> vRow = Avx2.LoadVector256(rPtr + j);
                                Avx2.Store(sPtr + j, Avx2.Add(vSum, vRow));
                            }
                            for (; j < cols; j++)
                                sPtr[j] += rPtr[j];
                        }
                    }
                });
                for (int t = 0; t < Environment.ProcessorCount; t++)
                    for (int j = 0; j < cols; j++)
                        sums[j] += localSums[t, j];
            }
            else
            {
                Parallel.For(0, rows, i =>
                {
                    for (int j = 0; j < cols; j++)
                        sums[j] += a[i, j];
                });
            }
            return sums;
        }

        /// <summary>Copy matrix.</summary>
        public static double[,] Copy(double[,] a)
        {
            int rows = a.GetLength(0), cols = a.GetLength(1);
            var c = new double[rows, cols];
            Array.Copy(a, c, a.Length);
            return c;
        }

        /// <summary>Get row as span (zero-allocation).</summary>
        public static Span<double> GetRowSpan(double[,] a, int row)
        {
            int cols = a.GetLength(1);
            unsafe
            {
                fixed (double* ptr = &a[row, 0])
                {
                    return new Span<double>(ptr, cols);
                }
            }
        }

        /// <summary>Get row as array.</summary>
        public static double[] GetRow(double[,] a, int row)
        {
            int cols = a.GetLength(1);
            var r = new double[cols];
            for (int j = 0; j < cols; j++) r[j] = a[row, j];
            return r;
        }

        /// <summary>Set row from array.</summary>
        public static void SetRow(double[,] a, int row, double[] values)
        {
            int cols = a.GetLength(1);
            for (int j = 0; j < cols; j++) a[row, j] = values[j];
        }

        /// <summary>Print matrix (debug).</summary>
        public static string ToString(double[,] a, int maxRows = 5, int maxCols = 5)
        {
            int rows = a.GetLength(0), cols = a.GetLength(1);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Matrix[{rows},{cols}]:");
            for (int i = 0; i < SysMath.Min(rows, maxRows); i++)
            {
                for (int j = 0; j < SysMath.Min(cols, maxCols); j++)
                    sb.Append($"{a[i, j]:F4} ");
                if (cols > maxCols) sb.Append("...");
                sb.AppendLine();
            }
            if (rows > maxRows) sb.AppendLine("...");
            return sb.ToString();
        }
    }
}