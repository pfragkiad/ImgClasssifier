namespace ImgClasssifier
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
            components = new System.ComponentModel.Container();
            pictureBox1 = new PictureBox();
            btnRefresh = new Button();
            trackBar1 = new TrackBar();
            toolTip1 = new ToolTip(components);
            btnSave = new Button();
            btnSkip = new Button();
            txtLog = new TextBox();
            chkRotateRight = new CheckBox();
            btnResetOrderInLogFile = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(698, 781);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(721, 12);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // trackBar1
            // 
            trackBar1.Location = new Point(721, 123);
            trackBar1.Maximum = 100;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(282, 45);
            trackBar1.TabIndex = 2;
            trackBar1.Value = 50;
            trackBar1.Scroll += trackBar1_Scroll;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(721, 174);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnSkip
            // 
            btnSkip.Location = new Point(721, 83);
            btnSkip.Name = "btnSkip";
            btnSkip.Size = new Size(75, 23);
            btnSkip.TabIndex = 1;
            btnSkip.Text = "Skip";
            btnSkip.UseVisualStyleBackColor = true;
            btnSkip.Click += btnSkip_Click;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txtLog.Location = new Point(721, 203);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(282, 590);
            txtLog.TabIndex = 4;
            // 
            // chkRotateRight
            // 
            chkRotateRight.AutoSize = true;
            chkRotateRight.Location = new Point(826, 16);
            chkRotateRight.Name = "chkRotateRight";
            chkRotateRight.Size = new Size(88, 19);
            chkRotateRight.TabIndex = 5;
            chkRotateRight.Text = "Rotate right";
            chkRotateRight.UseVisualStyleBackColor = true;
            chkRotateRight.CheckedChanged += chkRotateRight_CheckedChanged;
            // 
            // btnResetOrderInLogFile
            // 
            btnResetOrderInLogFile.Location = new Point(871, 83);
            btnResetOrderInLogFile.Name = "btnResetOrderInLogFile";
            btnResetOrderInLogFile.Size = new Size(132, 23);
            btnResetOrderInLogFile.TabIndex = 6;
            btnResetOrderInLogFile.Text = "Reset order in log file";
            btnResetOrderInLogFile.UseVisualStyleBackColor = true;
            btnResetOrderInLogFile.Click += btnResetOrderInLogFile_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1015, 805);
            Controls.Add(btnResetOrderInLogFile);
            Controls.Add(chkRotateRight);
            Controls.Add(txtLog);
            Controls.Add(btnSave);
            Controls.Add(trackBar1);
            Controls.Add(btnSkip);
            Controls.Add(btnRefresh);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Button btnRefresh;
        private TrackBar trackBar1;
        private ToolTip toolTip1;
        private Button btnSave;
        private Button btnSkip;
        private TextBox txtLog;
        private CheckBox chkRotateRight;
        private Button btnResetOrderInLogFile;
    }
}
