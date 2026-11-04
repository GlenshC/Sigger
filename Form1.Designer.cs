namespace Sigger
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabTrain = new TabPage();
            btnClearCache = new Button();
            btnTest = new Button();
            btnTrain = new Button();
            groupBoxHyperparams = new GroupBox();
            numEpochs = new NumericUpDown();
            lblEpochs = new Label();
            numBatchSize = new NumericUpDown();
            lblBatchSize = new Label();
            numHiddenSize = new NumericUpDown();
            lblHiddenSize = new Label();
            numLearningRate = new NumericUpDown();
            lblLearningRate = new Label();
            progressBarTraining = new ProgressBar();
            txtLog = new TextBox();
            lblLog = new Label();
            lblModelCache = new Label();
            cmbModelCache = new ComboBox();
            tabTest = new TabPage();
            btnEvaluate = new Button();
            dataGridConfusion = new DataGridView();
            lblConfusionMatrix = new Label();
            txtMetrics = new TextBox();
            lblMetrics = new Label();
            tabDraw = new TabPage();
            btnClearCanvas = new Button();
            lblDrawPrediction = new Label();
            txtDrawConfidence = new TextBox();
            lblDrawConfidence = new Label();
            lblBrushSize = new Label();
            numBrushSize = new NumericUpDown();
            picDrawCanvas = new PictureBox();
            tabControl.SuspendLayout();
            tabTrain.SuspendLayout();
            groupBoxHyperparams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numEpochs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numBatchSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHiddenSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLearningRate).BeginInit();
            tabTest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridConfusion).BeginInit();
            tabDraw.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picDrawCanvas).BeginInit();
            SuspendLayout();
            //
            // tabControl
            //
            tabControl.Controls.Add(tabTrain);
            tabControl.Controls.Add(tabTest);
            tabControl.Controls.Add(tabDraw);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(4, 4, 4, 4);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1250, 875);
            tabControl.TabIndex = 0;
            //
            // tabTrain
            //
            tabTrain.Controls.Add(lblModelCache);
            tabTrain.Controls.Add(cmbModelCache);
            tabTrain.Controls.Add(lblLearningRate);
            tabTrain.Controls.Add(groupBoxHyperparams);
            tabTrain.Controls.Add(btnTrain);
            tabTrain.Controls.Add(btnTest);
            tabTrain.Controls.Add(btnClearCache);
            tabTrain.Controls.Add(progressBarTraining);
            tabTrain.Controls.Add(lblLog);
            tabTrain.Controls.Add(txtLog);
            tabTrain.Location = new Point(4, 34);
            tabTrain.Margin = new Padding(4, 4, 4, 4);
            tabTrain.Name = "tabTrain";
            tabTrain.Padding = new Padding(4, 4, 4, 4);
            tabTrain.Size = new Size(1242, 837);
            tabTrain.TabIndex = 0;
            tabTrain.Text = "Train";
            tabTrain.UseVisualStyleBackColor = true;
            //
            // lblModelCache
            //
            lblModelCache.AutoSize = true;
            lblModelCache.Location = new Point(500, 25);
            lblModelCache.Margin = new Padding(4, 0, 4, 0);
            lblModelCache.Name = "lblModelCache";
            lblModelCache.Size = new Size(105, 25);
            lblModelCache.TabIndex = 8;
            lblModelCache.Text = "Model Cache:";
            //
            // cmbModelCache
            //
            cmbModelCache.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbModelCache.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbModelCache.FormattingEnabled = true;
            cmbModelCache.Location = new Point(610, 22);
            cmbModelCache.Margin = new Padding(4, 4, 4, 4);
            cmbModelCache.Name = "cmbModelCache";
            cmbModelCache.Size = new Size(600, 33);
            cmbModelCache.TabIndex = 7;
            cmbModelCache.SelectedIndexChanged += cmbModelCache_SelectedIndexChanged;
            //
            // btnClearCache
            //
            btnClearCache.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearCache.Location = new Point(341, 25);
            btnClearCache.Margin = new Padding(4, 4, 4, 4);
            btnClearCache.Name = "btnClearCache";
            btnClearCache.Size = new Size(150, 44);
            btnClearCache.TabIndex = 6;
            btnClearCache.Text = "Clear Cache";
            btnClearCache.UseVisualStyleBackColor = true;
            //
            // btnTest
            //
            btnTest.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTest.Enabled = false;
            btnTest.Location = new Point(183, 25);
            btnTest.Margin = new Padding(4, 4, 4, 4);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(150, 44);
            btnTest.TabIndex = 5;
            btnTest.Text = "Quick Test";
            btnTest.UseVisualStyleBackColor = true;
            //
            // btnTrain
            //
            btnTrain.Location = new Point(25, 25);
            btnTrain.Margin = new Padding(4, 4, 4, 4);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(150, 44);
            btnTrain.TabIndex = 4;
            btnTrain.Text = "Train Model";
            btnTrain.UseVisualStyleBackColor = true;
            //
            // groupBoxHyperparams
            //
            groupBoxHyperparams.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxHyperparams.Controls.Add(numLearningRate);
            groupBoxHyperparams.Controls.Add(lblHiddenSize);
            groupBoxHyperparams.Controls.Add(numHiddenSize);
            groupBoxHyperparams.Controls.Add(numEpochs);
            groupBoxHyperparams.Controls.Add(lblBatchSize);
            groupBoxHyperparams.Controls.Add(lblEpochs);
            groupBoxHyperparams.Controls.Add(numBatchSize);
            groupBoxHyperparams.Location = new Point(25, 88);
            groupBoxHyperparams.Margin = new Padding(4, 4, 4, 4);
            groupBoxHyperparams.Name = "groupBoxHyperparams";
            groupBoxHyperparams.Padding = new Padding(4, 4, 4, 4);
            groupBoxHyperparams.Size = new Size(918, 100);
            groupBoxHyperparams.TabIndex = 3;
            groupBoxHyperparams.TabStop = false;
            groupBoxHyperparams.Text = "Hyperparameters";
            //
            // numEpochs
            //
            numEpochs.Location = new Point(777, 43);
            numEpochs.Margin = new Padding(4, 4, 4, 4);
            numEpochs.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numEpochs.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numEpochs.Name = "numEpochs";
            numEpochs.Size = new Size(125, 31);
            numEpochs.TabIndex = 7;
            numEpochs.Value = new decimal(new int[] { 100, 0, 0, 0 });
            //
            // lblEpochs
            //
            lblEpochs.AutoSize = true;
            lblEpochs.Location = new Point(689, 45);
            lblEpochs.Margin = new Padding(4, 0, 4, 0);
            lblEpochs.Name = "lblEpochs";
            lblEpochs.Size = new Size(73, 25);
            lblEpochs.TabIndex = 6;
            lblEpochs.Text = "Epochs:";
            //
            // numBatchSize
            //
            numBatchSize.Location = new Point(539, 43);
            numBatchSize.Margin = new Padding(4, 4, 4, 4);
            numBatchSize.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            numBatchSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numBatchSize.Name = "numBatchSize";
            numBatchSize.Size = new Size(125, 31);
            numBatchSize.TabIndex = 5;
            numBatchSize.Value = new decimal(new int[] { 64, 0, 0, 0 });
            //
            // lblBatchSize
            //
            lblBatchSize.AutoSize = true;
            lblBatchSize.Location = new Point(427, 45);
            lblBatchSize.Margin = new Padding(4, 0, 4, 0);
            lblBatchSize.Name = "lblBatchSize";
            lblBatchSize.Size = new Size(95, 25);
            lblBatchSize.TabIndex = 4;
            lblBatchSize.Text = "Batch Size:";
            //
            // numHiddenSize
            //
            numHiddenSize.Location = new Point(289, 43);
            numHiddenSize.Margin = new Padding(4, 4, 4, 4);
            numHiddenSize.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
            numHiddenSize.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
            numHiddenSize.Name = "numHiddenSize";
            numHiddenSize.Size = new Size(125, 31);
            numHiddenSize.TabIndex = 3;
            numHiddenSize.Value = new decimal(new int[] { 256, 0, 0, 0 });
            //
            // lblHiddenSize
            //
            lblHiddenSize.AutoSize = true;
            lblHiddenSize.Location = new Point(165, 45);
            lblHiddenSize.Margin = new Padding(4, 0, 4, 0);
            lblHiddenSize.Name = "lblHiddenSize";
            lblHiddenSize.Size = new Size(110, 25);
            lblHiddenSize.TabIndex = 2;
            lblHiddenSize.Text = "Hidden Size:";
            //
            // numLearningRate
            //
            numLearningRate.DecimalPlaces = 4;
            numLearningRate.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numLearningRate.Location = new Point(38, 43);
            numLearningRate.Margin = new Padding(4, 4, 4, 4);
            numLearningRate.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numLearningRate.Minimum = new decimal(new int[] { 1, 0, 0, 262144 });
            numLearningRate.Name = "numLearningRate";
            numLearningRate.Size = new Size(112, 31);
            numLearningRate.TabIndex = 1;
            numLearningRate.Value = new decimal(new int[] { 1, 0, 0, 131072 });
            //
            // lblLearningRate
            //
            lblLearningRate.AutoSize = true;
            lblLearningRate.Location = new Point(12, 40);
            lblLearningRate.Margin = new Padding(4, 0, 4, 0);
            lblLearningRate.Name = "lblLearningRate";
            lblLearningRate.Size = new Size(0, 25);
            lblLearningRate.TabIndex = 0;
            //
            // progressBarTraining
            //
            progressBarTraining.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBarTraining.Location = new Point(25, 206);
            progressBarTraining.Margin = new Padding(4, 4, 4, 4);
            progressBarTraining.Name = "progressBarTraining";
            progressBarTraining.Size = new Size(1188, 38);
            progressBarTraining.TabIndex = 7;
            //
            // txtLog
            //
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.BackColor = Color.Black;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.ForeColor = Color.Lime;
            txtLog.Location = new Point(25, 273);
            txtLog.Margin = new Padding(4, 4, 4, 4);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(1186, 549);
            txtLog.TabIndex = 1;
            //
            // lblLog
            //
            lblLog.AutoSize = true;
            lblLog.Location = new Point(25, 244);
            lblLog.Margin = new Padding(4, 0, 4, 0);
            lblLog.Name = "lblLog";
            lblLog.Size = new Size(112, 25);
            lblLog.TabIndex = 0;
            lblLog.Text = "Training Log:";
            //
            // tabTest
            //
            tabTest.Controls.Add(btnEvaluate);
            tabTest.Controls.Add(dataGridConfusion);
            tabTest.Controls.Add(lblConfusionMatrix);
            tabTest.Controls.Add(txtMetrics);
            tabTest.Controls.Add(lblMetrics);
            tabTest.Location = new Point(4, 34);
            tabTest.Margin = new Padding(4, 4, 4, 4);
            tabTest.Name = "tabTest";
            tabTest.Padding = new Padding(4, 4, 4, 4);
            tabTest.Size = new Size(1242, 837);
            tabTest.TabIndex = 1;
            tabTest.Text = "Test";
            tabTest.UseVisualStyleBackColor = true;
            //
            // btnEvaluate
            //
            btnEvaluate.Location = new Point(25, 25);
            btnEvaluate.Margin = new Padding(4, 4, 4, 4);
            btnEvaluate.Name = "btnEvaluate";
            btnEvaluate.Size = new Size(188, 44);
            btnEvaluate.TabIndex = 4;
            btnEvaluate.Text = "Evaluate on Test Set";
            btnEvaluate.UseVisualStyleBackColor = true;
            //
            // dataGridConfusion
            //
            dataGridConfusion.AllowUserToAddRows = false;
            dataGridConfusion.AllowUserToDeleteRows = false;
            dataGridConfusion.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridConfusion.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridConfusion.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridConfusion.Location = new Point(25, 250);
            dataGridConfusion.Margin = new Padding(4, 4, 4, 4);
            dataGridConfusion.Name = "dataGridConfusion";
            dataGridConfusion.ReadOnly = true;
            dataGridConfusion.RowHeadersWidth = 51;
            dataGridConfusion.RowTemplate.Height = 29;
            dataGridConfusion.Size = new Size(1188, 562);
            dataGridConfusion.TabIndex = 3;
            //
            // lblConfusionMatrix
            //
            lblConfusionMatrix.AutoSize = true;
            lblConfusionMatrix.Location = new Point(25, 219);
            lblConfusionMatrix.Margin = new Padding(4, 0, 4, 0);
            lblConfusionMatrix.Name = "lblConfusionMatrix";
            lblConfusionMatrix.Size = new Size(151, 25);
            lblConfusionMatrix.TabIndex = 2;
            lblConfusionMatrix.Text = "Confusion Matrix:";
            //
            // txtMetrics
            //
            txtMetrics.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMetrics.BackColor = Color.White;
            txtMetrics.Font = new Font("Consolas", 10F);
            txtMetrics.Location = new Point(25, 88);
            txtMetrics.Margin = new Padding(4, 4, 4, 4);
            txtMetrics.Multiline = true;
            txtMetrics.Name = "txtMetrics";
            txtMetrics.ReadOnly = true;
            txtMetrics.ScrollBars = ScrollBars.Vertical;
            txtMetrics.Size = new Size(1186, 118);
            txtMetrics.TabIndex = 1;
            //
            // lblMetrics
            //
            lblMetrics.AutoSize = true;
            lblMetrics.Location = new Point(25, 81);
            lblMetrics.Margin = new Padding(4, 0, 4, 0);
            lblMetrics.Name = "lblMetrics";
            lblMetrics.Size = new Size(108, 25);
            lblMetrics.TabIndex = 0;
            lblMetrics.Text = "Test Metrics:";
            //
            // tabDraw
            //
            tabDraw.Controls.Add(btnClearCanvas);
            tabDraw.Controls.Add(lblDrawPrediction);
            tabDraw.Controls.Add(txtDrawConfidence);
            tabDraw.Controls.Add(lblDrawConfidence);
            tabDraw.Controls.Add(picDrawCanvas);
            tabDraw.Controls.Add(lblBrushSize);
            tabDraw.Controls.Add(numBrushSize);
            tabDraw.Location = new Point(4, 34);
            tabDraw.Margin = new Padding(4, 4, 4, 4);
            tabDraw.Name = "tabDraw";
            tabDraw.Padding = new Padding(4, 4, 4, 4);
            tabDraw.Size = new Size(1242, 837);
            tabDraw.TabIndex = 2;
            tabDraw.Text = "Draw Test";
            tabDraw.UseVisualStyleBackColor = true;
            //
            // btnClearCanvas
            //
            btnClearCanvas.Location = new Point(25, 25);
            btnClearCanvas.Margin = new Padding(4, 4, 4, 4);
            btnClearCanvas.Name = "btnClearCanvas";
            btnClearCanvas.Size = new Size(150, 44);
            btnClearCanvas.TabIndex = 5;
            btnClearCanvas.Text = "Clear Canvas";
            btnClearCanvas.UseVisualStyleBackColor = true;
            //
            // lblDrawPrediction
            //
            lblDrawPrediction.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblDrawPrediction.AutoSize = true;
            lblDrawPrediction.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblDrawPrediction.ForeColor = Color.Gray;
            lblDrawPrediction.Location = new Point(875, 88);
            lblDrawPrediction.Margin = new Padding(4, 0, 4, 0);
            lblDrawPrediction.Name = "lblDrawPrediction";
            lblDrawPrediction.Size = new Size(200, 41);
            lblDrawPrediction.TabIndex = 4;
            lblDrawPrediction.Text = "Draw a digit";
            //
            // txtDrawConfidence
            //
            txtDrawConfidence.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            txtDrawConfidence.BackColor = Color.White;
            txtDrawConfidence.Font = new Font("Consolas", 10F);
            txtDrawConfidence.Location = new Point(875, 150);
            txtDrawConfidence.Margin = new Padding(4, 4, 4, 4);
            txtDrawConfidence.Multiline = true;
            txtDrawConfidence.Name = "txtDrawConfidence";
            txtDrawConfidence.ReadOnly = true;
            txtDrawConfidence.ScrollBars = ScrollBars.Vertical;
            txtDrawConfidence.Size = new Size(349, 662);
            txtDrawConfidence.TabIndex = 3;
            //
            // lblDrawConfidence
            //
            lblDrawConfidence.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblDrawConfidence.AutoSize = true;
            lblDrawConfidence.Location = new Point(875, 129);
            lblDrawConfidence.Margin = new Padding(4, 0, 4, 0);
            lblDrawConfidence.Name = "lblDrawConfidence";
            lblDrawConfidence.Size = new Size(130, 25);
            lblDrawConfidence.TabIndex = 2;
            lblDrawConfidence.Text = "Probabilities:";
            //
            // picDrawCanvas
            //
            picDrawCanvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picDrawCanvas.BackColor = Color.White;
            picDrawCanvas.BorderStyle = BorderStyle.FixedSingle;
            picDrawCanvas.Location = new Point(25, 88);
            picDrawCanvas.Margin = new Padding(4, 4, 4, 4);
            picDrawCanvas.Name = "picDrawCanvas";
            picDrawCanvas.Size = new Size(400, 470);
            picDrawCanvas.TabIndex = 0;
            picDrawCanvas.TabStop = false;
            //
            // lblBrushSize
            //
            lblBrushSize.AutoSize = true;
            lblBrushSize.Location = new Point(185, 34);
            lblBrushSize.Margin = new Padding(4, 0, 4, 0);
            lblBrushSize.Name = "lblBrushSize";
            lblBrushSize.Size = new Size(88, 25);
            lblBrushSize.TabIndex = 6;
            lblBrushSize.Text = "Brush:";
            //
            // numBrushSize
            //
            numBrushSize.Location = new Point(280, 31);
            numBrushSize.Margin = new Padding(4, 4, 4, 4);
            numBrushSize.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numBrushSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numBrushSize.Name = "numBrushSize";
            numBrushSize.Size = new Size(80, 31);
            numBrushSize.TabIndex = 7;
            numBrushSize.Value = new decimal(new int[] { 12, 0, 0, 0 });
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1250, 875);
            Controls.Add(tabControl);
            Margin = new Padding(4, 4, 4, 4);
            MinimumSize = new Size(994, 736);
            Name = "Form1";
            Text = "Sigger - Digit MNIST Classifier";
            tabControl.ResumeLayout(false);
            tabTrain.ResumeLayout(false);
            tabTrain.PerformLayout();
            groupBoxHyperparams.ResumeLayout(false);
            groupBoxHyperparams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numEpochs).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBatchSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHiddenSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLearningRate).EndInit();
            tabTest.ResumeLayout(false);
            tabTest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridConfusion).EndInit();
            tabDraw.ResumeLayout(false);
            tabDraw.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picDrawCanvas).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ComboBox cmbModelCache;
        private System.Windows.Forms.Label lblModelCache;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabTrain;
        private System.Windows.Forms.TabPage tabTest;
        private System.Windows.Forms.TabPage tabDraw;
        private System.Windows.Forms.Button btnTrain;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.GroupBox groupBoxHyperparams;
        private System.Windows.Forms.NumericUpDown numLearningRate;
        private System.Windows.Forms.Label lblLearningRate;
        private System.Windows.Forms.NumericUpDown numHiddenSize;
        private System.Windows.Forms.Label lblHiddenSize;
        private System.Windows.Forms.NumericUpDown numBatchSize;
        private System.Windows.Forms.Label lblBatchSize;
        private System.Windows.Forms.NumericUpDown numEpochs;
        private System.Windows.Forms.Label lblEpochs;
        private System.Windows.Forms.ProgressBar progressBarTraining;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.Button btnEvaluate;
        private System.Windows.Forms.DataGridView dataGridConfusion;
        private System.Windows.Forms.Label lblConfusionMatrix;
        private System.Windows.Forms.TextBox txtMetrics;
        private System.Windows.Forms.Label lblMetrics;
        private System.Windows.Forms.Button btnClearCanvas;
        private System.Windows.Forms.Label lblDrawPrediction;
        private System.Windows.Forms.TextBox txtDrawConfidence;
        private System.Windows.Forms.Label lblDrawConfidence;
        private System.Windows.Forms.PictureBox picDrawCanvas;
        private System.Windows.Forms.Label lblBrushSize;
        private System.Windows.Forms.NumericUpDown numBrushSize;
    }
}