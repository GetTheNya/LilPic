using System.Windows.Forms;

namespace BulkImageCompressor {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.directoryBox = new System.Windows.Forms.GroupBox();
            this.backupLabel = new System.Windows.Forms.Label();
            this.overrideCBox = new System.Windows.Forms.CheckBox();
            this.compressChildrenCBox = new System.Windows.Forms.CheckBox();
            this.chooseFolderBtn = new System.Windows.Forms.Button();
            this.directoryPathInput = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.compressCompressedCBox = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.saveWEBPRButton = new System.Windows.Forms.RadioButton();
            this.savePNGRButton = new System.Windows.Forms.RadioButton();
            this.saveJPEGRButton = new System.Windows.Forms.RadioButton();
            this.KBMKBox = new System.Windows.Forms.ComboBox();
            this.fileSizeInput = new System.Windows.Forms.TextBox();
            this.bigFileCBox = new System.Windows.Forms.CheckBox();
            this.resizeNumeric = new System.Windows.Forms.NumericUpDown();
            this.resizeLabel = new System.Windows.Forms.Label();
            this.resizeTrackBar = new System.Windows.Forms.TrackBar();
            this.qualityNumeric = new System.Windows.Forms.NumericUpDown();
            this.qualityLabel = new System.Windows.Forms.Label();
            this.qualityTrackBar = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chooseSaveFolderBtn = new System.Windows.Forms.Button();
            this.savePathInput = new System.Windows.Forms.TextBox();
            this.compressBtn = new System.Windows.Forms.Button();
            this.directoryBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resizeNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resizeTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityTrackBar)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // directoryBox
            // 
            this.directoryBox.Controls.Add(this.backupLabel);
            this.directoryBox.Controls.Add(this.overrideCBox);
            this.directoryBox.Controls.Add(this.compressChildrenCBox);
            this.directoryBox.Controls.Add(this.chooseFolderBtn);
            this.directoryBox.Controls.Add(this.directoryPathInput);
            this.directoryBox.Location = new System.Drawing.Point(12, 12);
            this.directoryBox.Name = "directoryBox";
            this.directoryBox.Size = new System.Drawing.Size(776, 71);
            this.directoryBox.TabIndex = 0;
            this.directoryBox.TabStop = false;
            this.directoryBox.Text = "Directory to compress";
            // 
            // backupLabel
            // 
            this.backupLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.backupLabel.Location = new System.Drawing.Point(376, 42);
            this.backupLabel.Name = "backupLabel";
            this.backupLabel.Size = new System.Drawing.Size(394, 26);
            this.backupLabel.TabIndex = 4;
            this.backupLabel.Text = "ENSHURE YOU HAVE TAKEN BACKUP FIRST!";
            this.backupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.backupLabel.Visible = false;
            // 
            // overrideCBox
            // 
            this.overrideCBox.Location = new System.Drawing.Point(223, 44);
            this.overrideCBox.Name = "overrideCBox";
            this.overrideCBox.Size = new System.Drawing.Size(147, 21);
            this.overrideCBox.TabIndex = 3;
            this.overrideCBox.Text = "Overwrite existing images";
            this.overrideCBox.UseVisualStyleBackColor = true;
            this.overrideCBox.CheckedChanged += new System.EventHandler(this.overrideCBox_CheckedChanged);
            // 
            // compressChildrenCBox
            // 
            this.compressChildrenCBox.Location = new System.Drawing.Point(7, 44);
            this.compressChildrenCBox.Name = "compressChildrenCBox";
            this.compressChildrenCBox.Size = new System.Drawing.Size(210, 21);
            this.compressChildrenCBox.TabIndex = 2;
            this.compressChildrenCBox.Text = "Compress images of all child directories";
            this.compressChildrenCBox.UseVisualStyleBackColor = true;
            // 
            // chooseFolderBtn
            // 
            this.chooseFolderBtn.Location = new System.Drawing.Point(736, 17);
            this.chooseFolderBtn.Name = "chooseFolderBtn";
            this.chooseFolderBtn.Size = new System.Drawing.Size(34, 23);
            this.chooseFolderBtn.TabIndex = 1;
            this.chooseFolderBtn.Text = "...";
            this.chooseFolderBtn.UseVisualStyleBackColor = true;
            this.chooseFolderBtn.Click += new System.EventHandler(this.chooseFolderBtn_Click);
            // 
            // directoryPathInput
            // 
            this.directoryPathInput.Location = new System.Drawing.Point(6, 19);
            this.directoryPathInput.Name = "directoryPathInput";
            this.directoryPathInput.Size = new System.Drawing.Size(724, 20);
            this.directoryPathInput.TabIndex = 0;
            this.directoryPathInput.TextChanged += new System.EventHandler(this.directoryPathInput_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.compressCompressedCBox);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.KBMKBox);
            this.groupBox1.Controls.Add(this.fileSizeInput);
            this.groupBox1.Controls.Add(this.bigFileCBox);
            this.groupBox1.Controls.Add(this.resizeNumeric);
            this.groupBox1.Controls.Add(this.resizeLabel);
            this.groupBox1.Controls.Add(this.resizeTrackBar);
            this.groupBox1.Controls.Add(this.qualityNumeric);
            this.groupBox1.Controls.Add(this.qualityLabel);
            this.groupBox1.Controls.Add(this.qualityTrackBar);
            this.groupBox1.Location = new System.Drawing.Point(12, 89);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(776, 138);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Compression parameters";
            // 
            // compressCompressedCBox
            // 
            this.compressCompressedCBox.Location = new System.Drawing.Point(447, 46);
            this.compressCompressedCBox.Name = "compressCompressedCBox";
            this.compressCompressedCBox.Size = new System.Drawing.Size(209, 21);
            this.compressCompressedCBox.TabIndex = 10;
            this.compressCompressedCBox.Text = "Compress already compressed images";
            this.compressCompressedCBox.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.saveWEBPRButton);
            this.panel1.Controls.Add(this.savePNGRButton);
            this.panel1.Controls.Add(this.saveJPEGRButton);
            this.panel1.Location = new System.Drawing.Point(447, 70);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(318, 62);
            this.panel1.TabIndex = 9;
            // 
            // saveWEBPRButton
            // 
            this.saveWEBPRButton.Location = new System.Drawing.Point(7, 33);
            this.saveWEBPRButton.Name = "saveWEBPRButton";
            this.saveWEBPRButton.Size = new System.Drawing.Size(106, 24);
            this.saveWEBPRButton.TabIndex = 2;
            this.saveWEBPRButton.TabStop = true;
            this.saveWEBPRButton.Text = "Save as webp";
            this.saveWEBPRButton.UseVisualStyleBackColor = true;
            // 
            // savePNGRButton
            // 
            this.savePNGRButton.Location = new System.Drawing.Point(175, 3);
            this.savePNGRButton.Name = "savePNGRButton";
            this.savePNGRButton.Size = new System.Drawing.Size(86, 24);
            this.savePNGRButton.TabIndex = 1;
            this.savePNGRButton.TabStop = true;
            this.savePNGRButton.Text = "Save as png";
            this.savePNGRButton.UseVisualStyleBackColor = true;
            // 
            // saveJPEGRButton
            // 
            this.saveJPEGRButton.Checked = true;
            this.saveJPEGRButton.Location = new System.Drawing.Point(7, 3);
            this.saveJPEGRButton.Name = "saveJPEGRButton";
            this.saveJPEGRButton.Size = new System.Drawing.Size(162, 24);
            this.saveJPEGRButton.TabIndex = 0;
            this.saveJPEGRButton.TabStop = true;
            this.saveJPEGRButton.Text = "Save as JPEG (recomended)";
            this.saveJPEGRButton.UseVisualStyleBackColor = true;
            // 
            // KBMKBox
            // 
            this.KBMKBox.Enabled = false;
            this.KBMKBox.FormattingEnabled = true;
            this.KBMKBox.Items.AddRange(new object[] {
            "KB",
            "MB"});
            this.KBMKBox.Location = new System.Drawing.Point(707, 17);
            this.KBMKBox.Name = "KBMKBox";
            this.KBMKBox.Size = new System.Drawing.Size(58, 21);
            this.KBMKBox.TabIndex = 8;
            // 
            // fileSizeInput
            // 
            this.fileSizeInput.Enabled = false;
            this.fileSizeInput.Location = new System.Drawing.Point(626, 18);
            this.fileSizeInput.Name = "fileSizeInput";
            this.fileSizeInput.Size = new System.Drawing.Size(75, 20);
            this.fileSizeInput.TabIndex = 7;
            // 
            // bigFileCBox
            // 
            this.bigFileCBox.Location = new System.Drawing.Point(447, 18);
            this.bigFileCBox.Name = "bigFileCBox";
            this.bigFileCBox.Size = new System.Drawing.Size(173, 21);
            this.bigFileCBox.TabIndex = 6;
            this.bigFileCBox.Text = "Compress if size is greater than\r\n";
            this.bigFileCBox.UseVisualStyleBackColor = true;
            this.bigFileCBox.CheckedChanged += new System.EventHandler(this.bigFileCBox_CheckedChanged);
            // 
            // resizeNumeric
            // 
            this.resizeNumeric.Location = new System.Drawing.Point(291, 63);
            this.resizeNumeric.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.resizeNumeric.Name = "resizeNumeric";
            this.resizeNumeric.Size = new System.Drawing.Size(43, 20);
            this.resizeNumeric.TabIndex = 5;
            this.resizeNumeric.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            this.resizeNumeric.ValueChanged += new System.EventHandler(this.resizeNumeric_ValueChanged);
            // 
            // resizeLabel
            // 
            this.resizeLabel.Location = new System.Drawing.Point(7, 65);
            this.resizeLabel.Name = "resizeLabel";
            this.resizeLabel.Size = new System.Drawing.Size(41, 15);
            this.resizeLabel.TabIndex = 4;
            this.resizeLabel.Text = "Resize";
            // 
            // resizeTrackBar
            // 
            this.resizeTrackBar.LargeChange = 1;
            this.resizeTrackBar.Location = new System.Drawing.Point(54, 63);
            this.resizeTrackBar.Maximum = 100;
            this.resizeTrackBar.Minimum = 10;
            this.resizeTrackBar.Name = "resizeTrackBar";
            this.resizeTrackBar.Size = new System.Drawing.Size(231, 45);
            this.resizeTrackBar.TabIndex = 3;
            this.resizeTrackBar.TabStop = false;
            this.resizeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.resizeTrackBar.Value = 80;
            this.resizeTrackBar.Scroll += new System.EventHandler(this.resizeTrackBar_Scroll);
            // 
            // qualityNumeric
            // 
            this.qualityNumeric.Location = new System.Drawing.Point(291, 19);
            this.qualityNumeric.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.qualityNumeric.Name = "qualityNumeric";
            this.qualityNumeric.Size = new System.Drawing.Size(43, 20);
            this.qualityNumeric.TabIndex = 2;
            this.qualityNumeric.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            this.qualityNumeric.ValueChanged += new System.EventHandler(this.qualityNumeric_ValueChanged);
            // 
            // qualityLabel
            // 
            this.qualityLabel.Location = new System.Drawing.Point(7, 21);
            this.qualityLabel.Name = "qualityLabel";
            this.qualityLabel.Size = new System.Drawing.Size(41, 15);
            this.qualityLabel.TabIndex = 1;
            this.qualityLabel.Text = "Quality";
            // 
            // qualityTrackBar
            // 
            this.qualityTrackBar.LargeChange = 1;
            this.qualityTrackBar.Location = new System.Drawing.Point(54, 19);
            this.qualityTrackBar.Maximum = 100;
            this.qualityTrackBar.Minimum = 10;
            this.qualityTrackBar.Name = "qualityTrackBar";
            this.qualityTrackBar.Size = new System.Drawing.Size(231, 45);
            this.qualityTrackBar.TabIndex = 0;
            this.qualityTrackBar.TabStop = false;
            this.qualityTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.qualityTrackBar.Value = 80;
            this.qualityTrackBar.Scroll += new System.EventHandler(this.qualityTrackBar_Scroll);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chooseSaveFolderBtn);
            this.groupBox2.Controls.Add(this.savePathInput);
            this.groupBox2.Location = new System.Drawing.Point(12, 235);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(776, 54);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Save compressed images to directory";
            // 
            // chooseSaveFolderBtn
            // 
            this.chooseSaveFolderBtn.Location = new System.Drawing.Point(737, 17);
            this.chooseSaveFolderBtn.Name = "chooseSaveFolderBtn";
            this.chooseSaveFolderBtn.Size = new System.Drawing.Size(34, 23);
            this.chooseSaveFolderBtn.TabIndex = 3;
            this.chooseSaveFolderBtn.Text = "...";
            this.chooseSaveFolderBtn.UseVisualStyleBackColor = true;
            this.chooseSaveFolderBtn.Click += new System.EventHandler(this.chooseSaveFolderBtn_Click);
            // 
            // savePathInput
            // 
            this.savePathInput.Location = new System.Drawing.Point(7, 19);
            this.savePathInput.Name = "savePathInput";
            this.savePathInput.Size = new System.Drawing.Size(724, 20);
            this.savePathInput.TabIndex = 2;
            // 
            // compressBtn
            // 
            this.compressBtn.Location = new System.Drawing.Point(668, 295);
            this.compressBtn.Name = "compressBtn";
            this.compressBtn.Size = new System.Drawing.Size(120, 39);
            this.compressBtn.TabIndex = 3;
            this.compressBtn.Text = "Compress";
            this.compressBtn.UseVisualStyleBackColor = true;
            this.compressBtn.Click += new System.EventHandler(this.compressBtn_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 345);
            this.Controls.Add(this.compressBtn);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.directoryBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bulk image compressor";
            this.directoryBox.ResumeLayout(false);
            this.directoryBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.resizeNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resizeTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityTrackBar)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.RadioButton saveWEBPRButton;

        private System.Windows.Forms.CheckBox compressCompressedCBox;

        private System.Windows.Forms.Button chooseSaveFolderBtn;

        private System.Windows.Forms.Button compressBtn;
        private System.Windows.Forms.TextBox savePathInput;

        private System.Windows.Forms.GroupBox groupBox2;

        private System.Windows.Forms.Panel panel1;

        private System.Windows.Forms.RadioButton saveJPEGRButton;
        private System.Windows.Forms.RadioButton savePNGRButton;

        private System.Windows.Forms.ComboBox KBMKBox;

        private System.Windows.Forms.TextBox fileSizeInput;

        private System.Windows.Forms.CheckBox bigFileCBox;

        private System.Windows.Forms.Label qualityLabel;

        private System.Windows.Forms.NumericUpDown qualityNumeric;

        private System.Windows.Forms.TrackBar qualityTrackBar;

        private System.Windows.Forms.NumericUpDown resizeNumeric;

        private System.Windows.Forms.Label resizeLabel;

        private System.Windows.Forms.TrackBar resizeTrackBar;

        private System.Windows.Forms.GroupBox groupBox1;

        private System.Windows.Forms.Label backupLabel;

        private System.Windows.Forms.CheckBox overrideCBox;

        private System.Windows.Forms.CheckBox compressChildrenCBox;

        private System.Windows.Forms.TextBox directoryPathInput;
        private System.Windows.Forms.Button chooseFolderBtn;

        private System.Windows.Forms.GroupBox directoryBox;

        #endregion
    }
}