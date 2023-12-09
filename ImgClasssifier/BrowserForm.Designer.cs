namespace ImgClasssifier
{
    partial class BrowserForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            listView1 = new ListView();
            imageList1 = new ImageList(components);
            picPreview = new PictureBox();
            progressBar1 = new ProgressBar();
            trackBar1 = new TrackBar();
            chkMoveAfterChangeRating = new CheckBox();
            btnDelete = new Button();
            btnRerateUniformly = new Button();
            btnChangeRating = new Button();
            lblReport = new Label();
            toolTip1 = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(picPreview);
            splitContainer1.Panel2.Controls.Add(progressBar1);
            splitContainer1.Panel2.Controls.Add(trackBar1);
            splitContainer1.Panel2.Controls.Add(chkMoveAfterChangeRating);
            splitContainer1.Panel2.Controls.Add(btnDelete);
            splitContainer1.Panel2.Controls.Add(btnRerateUniformly);
            splitContainer1.Panel2.Controls.Add(btnChangeRating);
            splitContainer1.Panel2.Controls.Add(lblReport);
            splitContainer1.Size = new Size(1316, 892);
            splitContainer1.SplitterDistance = 753;
            splitContainer1.TabIndex = 1;
            // 
            // listView1
            // 
            listView1.Dock = DockStyle.Fill;
            listView1.LargeImageList = imageList1;
            listView1.Location = new Point(0, 0);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new Size(753, 892);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageSize = new Size(128, 256);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // picPreview
            // 
            picPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picPreview.BorderStyle = BorderStyle.FixedSingle;
            picPreview.Location = new Point(15, 254);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(532, 626);
            picPreview.TabIndex = 10;
            picPreview.TabStop = false;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(15, 160);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(532, 23);
            progressBar1.TabIndex = 9;
            progressBar1.Visible = false;
            // 
            // trackBar1
            // 
            trackBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBar1.Location = new Point(15, 203);
            trackBar1.Maximum = 100;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(532, 45);
            trackBar1.TabIndex = 4;
            trackBar1.TickStyle = TickStyle.Both;
            trackBar1.Value = 50;
            trackBar1.ValueChanged += trackBar1_ValueChanged;
            // 
            // chkMoveAfterChangeRating
            // 
            chkMoveAfterChangeRating.AutoSize = true;
            chkMoveAfterChangeRating.Location = new Point(15, 85);
            chkMoveAfterChangeRating.Name = "chkMoveAfterChangeRating";
            chkMoveAfterChangeRating.Size = new Size(229, 19);
            chkMoveAfterChangeRating.TabIndex = 8;
            chkMoveAfterChangeRating.Text = "Move to new place after change rating";
            chkMoveAfterChangeRating.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(442, 16);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(105, 23);
            btnDelete.TabIndex = 7;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnRerateUniformly
            // 
            btnRerateUniformly.Location = new Point(15, 131);
            btnRerateUniformly.Name = "btnRerateUniformly";
            btnRerateUniformly.Size = new Size(105, 23);
            btnRerateUniformly.TabIndex = 6;
            btnRerateUniformly.Text = "Rate uniformly";
            btnRerateUniformly.UseVisualStyleBackColor = true;
            btnRerateUniformly.Click += btnRerateUniformly_Click;
            // 
            // btnChangeRating
            // 
            btnChangeRating.Location = new Point(15, 57);
            btnChangeRating.Name = "btnChangeRating";
            btnChangeRating.Size = new Size(105, 23);
            btnChangeRating.TabIndex = 5;
            btnChangeRating.Text = "Change Rating";
            btnChangeRating.UseVisualStyleBackColor = true;
            btnChangeRating.Click += btnChangeRating_Click;
            // 
            // lblReport
            // 
            lblReport.AutoSize = true;
            lblReport.Location = new Point(15, 20);
            lblReport.Name = "lblReport";
            lblReport.Size = new Size(38, 15);
            lblReport.TabIndex = 2;
            lblReport.Text = "label1";
            // 
            // BrowserForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1316, 892);
            Controls.Add(splitContainer1);
            Name = "BrowserForm";
            Text = "BrowserForm";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private ListView listView1;
        private ImageList imageList1;
        private Label lblReport;
        private Button btnChangeRating;
        private TrackBar trackBar1;
        private ToolTip toolTip1;
        private Button btnRerateUniformly;
        private Button btnDelete;
        private CheckBox chkMoveAfterChangeRating;
        private ProgressBar progressBar1;
        private PictureBox picPreview;
    }
}