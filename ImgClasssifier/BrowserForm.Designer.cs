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
            btnRerateUniformly = new Button();
            btnChangeRating = new Button();
            trackBar1 = new TrackBar();
            lblReport = new Label();
            toolTip1 = new ToolTip(components);
            btnDelete = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
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
            splitContainer1.Panel2.Controls.Add(btnDelete);
            splitContainer1.Panel2.Controls.Add(btnRerateUniformly);
            splitContainer1.Panel2.Controls.Add(btnChangeRating);
            splitContainer1.Panel2.Controls.Add(trackBar1);
            splitContainer1.Panel2.Controls.Add(lblReport);
            splitContainer1.Size = new Size(1154, 722);
            splitContainer1.SplitterDistance = 810;
            splitContainer1.TabIndex = 1;
            // 
            // listView1
            // 
            listView1.Dock = DockStyle.Fill;
            listView1.LargeImageList = imageList1;
            listView1.Location = new Point(0, 0);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new Size(810, 722);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.KeyDown += listView1_KeyDown;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageSize = new Size(128, 256);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // btnRerateUniformly
            // 
            btnRerateUniformly.Location = new Point(84, 124);
            btnRerateUniformly.Name = "btnRerateUniformly";
            btnRerateUniformly.Size = new Size(105, 23);
            btnRerateUniformly.TabIndex = 6;
            btnRerateUniformly.Text = "Rate uniformly";
            btnRerateUniformly.UseVisualStyleBackColor = true;
            btnRerateUniformly.Click += btnRerateUniformly_Click;
            // 
            // btnChangeRating
            // 
            btnChangeRating.Location = new Point(84, 71);
            btnChangeRating.Name = "btnChangeRating";
            btnChangeRating.Size = new Size(105, 23);
            btnChangeRating.TabIndex = 5;
            btnChangeRating.Text = "Change Rating";
            btnChangeRating.UseVisualStyleBackColor = true;
            btnChangeRating.Click += btnChangeRating_Click;
            // 
            // trackBar1
            // 
            trackBar1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            trackBar1.Location = new Point(15, 56);
            trackBar1.Maximum = 100;
            trackBar1.Name = "trackBar1";
            trackBar1.Orientation = Orientation.Vertical;
            trackBar1.Size = new Size(45, 622);
            trackBar1.TabIndex = 4;
            trackBar1.TickStyle = TickStyle.Both;
            trackBar1.Value = 50;
            trackBar1.ValueChanged += trackBar1_ValueChanged;
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
            // btnDelete
            // 
            btnDelete.Location = new Point(209, 218);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(105, 23);
            btnDelete.TabIndex = 7;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // BrowserForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1154, 722);
            Controls.Add(splitContainer1);
            Name = "BrowserForm";
            Text = "BrowserForm";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
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
    }
}