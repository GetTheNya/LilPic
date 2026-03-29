namespace LilPic.UI;

partial class MainForm {
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent() {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        this.splitContainer = new System.Windows.Forms.SplitContainer();
        this.fileTreePanel = new LilPic.UI.FileTreePanel();
        this.settingsPanel = new System.Windows.Forms.Panel();
        this.presetsLabel = new System.Windows.Forms.Label();
        this.presetsCombo = new System.Windows.Forms.ComboBox();
        this.directoryBox = new System.Windows.Forms.GroupBox();
        this.chooseFolderBtn = new System.Windows.Forms.Button();
        this.directoryPathInput = new System.Windows.Forms.TextBox();
        this.groupBox1 = new System.Windows.Forms.GroupBox();
        this.targetSizePanel = new System.Windows.Forms.Panel();
        this.labelTargetSize = new System.Windows.Forms.Label();
        this.targetSizeInput = new System.Windows.Forms.TextBox();
        this.targetResPanel = new System.Windows.Forms.Panel();
        this.labelW = new System.Windows.Forms.Label();
        this.labelH = new System.Windows.Forms.Label();
        this.targetWidthInput = new System.Windows.Forms.NumericUpDown();
        this.targetHeightInput = new System.Windows.Forms.NumericUpDown();
        this.resizeModeCombo = new System.Windows.Forms.ComboBox();
        this.stripMetadataCBox = new System.Windows.Forms.CheckBox();
        this.copyNonImagesCBox = new System.Windows.Forms.CheckBox();
        this.overwritePolicyLabel = new System.Windows.Forms.Label();
        this.overwritePolicyCombo = new System.Windows.Forms.ComboBox();
        this.compressCompressedCBox = new System.Windows.Forms.CheckBox();
        this.copySkippedCBox = new System.Windows.Forms.CheckBox();
        this.formatPanel = new System.Windows.Forms.Panel();
        this.saveWEBPRButton = new System.Windows.Forms.RadioButton();
        this.savePNGRButton = new System.Windows.Forms.RadioButton();
        this.saveJPEGRButton = new System.Windows.Forms.RadioButton();
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
        this.dryRunBtn = new System.Windows.Forms.Button();
        this.statusStrip = new System.Windows.Forms.StatusStrip();
        this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
        this.versionLabel = new System.Windows.Forms.ToolStripStatusLabel();
        this.aboutLink = new System.Windows.Forms.ToolStripStatusLabel();
        
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
        this.splitContainer.Panel1.SuspendLayout();
        this.splitContainer.Panel2.SuspendLayout();
        this.splitContainer.SuspendLayout();
        this.directoryBox.SuspendLayout();
        this.groupBox1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.targetWidthInput)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.targetHeightInput)).BeginInit();
        this.formatPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.resizeNumeric)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.resizeTrackBar)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.qualityNumeric)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.qualityTrackBar)).BeginInit();
        this.groupBox2.SuspendLayout();
        this.statusStrip.SuspendLayout();
        this.SuspendLayout();

        // 
        // splitContainer
        // 
        this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitContainer.Location = new System.Drawing.Point(0, 0);
        this.splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        this.splitContainer.Panel1.Controls.Add(this.fileTreePanel);
        // 
        // splitContainer.Panel2
        // 
        this.splitContainer.Panel2.Controls.Add(this.settingsPanel);
        this.splitContainer.Size = new System.Drawing.Size(1100, 650);
        this.splitContainer.SplitterDistance = 600;

        // 
        // fileTreePanel
        // 
        this.fileTreePanel.Dock = System.Windows.Forms.DockStyle.Fill;

        // 
        // settingsPanel
        // 
        this.settingsPanel.Controls.Add(this.presetsLabel);
        this.settingsPanel.Controls.Add(this.presetsCombo);
        this.settingsPanel.Controls.Add(this.directoryBox);
        this.settingsPanel.Controls.Add(this.groupBox1);
        this.settingsPanel.Controls.Add(this.groupBox2);
        this.settingsPanel.Controls.Add(this.compressBtn);
        this.settingsPanel.Controls.Add(this.dryRunBtn);
        this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.settingsPanel.Padding = new System.Windows.Forms.Padding(10);

        // Presets
        this.presetsLabel.Location = new System.Drawing.Point(20, 15);
        this.presetsLabel.Text = "Preset:";
        this.presetsLabel.Size = new System.Drawing.Size(50, 20);
        this.presetsCombo.Location = new System.Drawing.Point(80, 12);
        this.presetsCombo.Size = new System.Drawing.Size(370, 21);

        // Directory Box
        this.directoryBox.Location = new System.Drawing.Point(10, 45);
        this.directoryBox.Size = new System.Drawing.Size(460, 65);
        this.directoryBox.Text = "Input Directory";
        this.directoryPathInput.Location = new System.Drawing.Point(10, 25);
        this.directoryPathInput.Size = new System.Drawing.Size(400, 20);
        this.chooseFolderBtn.Location = new System.Drawing.Point(415, 23);
        this.chooseFolderBtn.Size = new System.Drawing.Size(35, 23);
        this.presetsCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.presetsCombo.Items.AddRange(new object[] { "Custom", "Web Optimized (JPEG)", "High Quality (PNG)", "Max Compression (WEBP)", "Social Media (Small)" });

        this.directoryBox.Controls.Add(this.directoryPathInput);
        this.directoryBox.Controls.Add(this.chooseFolderBtn);

        // GroupBox 1 (Parameters)
        this.groupBox1.Location = new System.Drawing.Point(10, 120);
        this.groupBox1.Size = new System.Drawing.Size(460, 440);
        this.groupBox1.Text = "Compression Parameters";

        this.qualityLabel.Location = new System.Drawing.Point(10, 30);
        this.qualityLabel.Text = "Quality";
        this.qualityLabel.Size = new System.Drawing.Size(50, 20);
        this.qualityTrackBar.Location = new System.Drawing.Point(60, 25);
        this.qualityTrackBar.Size = new System.Drawing.Size(320, 45);
        this.qualityTrackBar.Minimum = 0;
        this.qualityTrackBar.Maximum = 100;
        this.qualityNumeric.Location = new System.Drawing.Point(390, 25);
        this.qualityNumeric.Size = new System.Drawing.Size(50, 20);
        this.qualityNumeric.Minimum = 0;
        this.qualityNumeric.Maximum = 100;

        this.groupBox1.Controls.Add(this.qualityLabel);
        this.groupBox1.Controls.Add(this.qualityTrackBar);
        this.groupBox1.Controls.Add(this.qualityNumeric);

        this.resizeModeCombo.Location = new System.Drawing.Point(10, 80);
        this.resizeModeCombo.Size = new System.Drawing.Size(150, 21);
        this.resizeModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.resizeModeCombo.Items.AddRange(new object[] { "Resize (%)", "Target Res (px)", "Target Size (KB)" });

        this.resizeLabel.Location = new System.Drawing.Point(170, 83);
        this.resizeLabel.Text = "%";
        this.resizeLabel.Size = new System.Drawing.Size(20, 20);
        this.resizeTrackBar.Location = new System.Drawing.Point(10, 115);
        this.resizeTrackBar.Size = new System.Drawing.Size(370, 45);
        this.resizeTrackBar.Minimum = 0;
        this.resizeTrackBar.Maximum = 100;
        this.resizeNumeric.Location = new System.Drawing.Point(390, 115);
        this.resizeNumeric.Size = new System.Drawing.Size(50, 20);
        this.resizeNumeric.Minimum = 0;
        this.resizeNumeric.Maximum = 100;

        this.groupBox1.Controls.Add(this.resizeModeCombo);
        this.groupBox1.Controls.Add(this.resizeLabel);
        this.groupBox1.Controls.Add(this.resizeTrackBar);
        this.groupBox1.Controls.Add(this.resizeNumeric);

        // Target Size Panel
        this.targetSizePanel.Location = new System.Drawing.Point(10, 110);
        this.targetSizePanel.Size = new System.Drawing.Size(400, 40);
        this.labelTargetSize.Location = new System.Drawing.Point(0, 5);
        this.labelTargetSize.Text = "Target size:";
        this.labelTargetSize.Size = new System.Drawing.Size(70, 20);
        this.targetSizeInput.Location = new System.Drawing.Point(75, 2);
        this.targetSizeInput.Size = new System.Drawing.Size(80, 20);
        
        Label labelKB = new Label();
        labelKB.Text = "KB";
        labelKB.Location = new System.Drawing.Point(160, 5);
        labelKB.Size = new System.Drawing.Size(30, 20);

        this.targetSizePanel.Controls.Add(this.labelTargetSize);
        this.targetSizePanel.Controls.Add(this.targetSizeInput);
        this.targetSizePanel.Controls.Add(labelKB);
        this.targetSizePanel.Visible = false;

        // Target Res Panel
        this.targetResPanel.Location = new System.Drawing.Point(10, 110);
        this.targetResPanel.Size = new System.Drawing.Size(400, 40);
        this.labelW.Text = "W:";
        this.labelW.Location = new System.Drawing.Point(0, 5);
        this.labelW.Size = new System.Drawing.Size(20, 20);
        this.targetWidthInput.Location = new System.Drawing.Point(25, 2);
        this.targetWidthInput.Size = new System.Drawing.Size(70, 20);
        this.targetWidthInput.Maximum = 99999;
        this.labelH.Text = "H:";
        this.labelH.Location = new System.Drawing.Point(110, 5);
        this.labelH.Size = new System.Drawing.Size(20, 20);
        this.targetHeightInput.Location = new System.Drawing.Point(135, 2);
        this.targetHeightInput.Size = new System.Drawing.Size(70, 20);
        this.targetHeightInput.Maximum = 99999;
        this.targetResPanel.Controls.Add(this.labelW);
        this.targetResPanel.Controls.Add(this.targetWidthInput);
        this.targetResPanel.Controls.Add(this.labelH);
        this.targetResPanel.Controls.Add(this.targetHeightInput);
        this.targetResPanel.Visible = false;

        this.groupBox1.Controls.Add(this.targetSizePanel);
        this.groupBox1.Controls.Add(this.targetResPanel);

        this.stripMetadataCBox.Location = new System.Drawing.Point(10, 160);
        this.stripMetadataCBox.Text = "Strip Metadata (GPS, Camera info)";
        this.stripMetadataCBox.Size = new System.Drawing.Size(300, 20);
        this.copyNonImagesCBox.Location = new System.Drawing.Point(10, 185);
        this.copyNonImagesCBox.Text = "Copy non-images to output";
        this.copyNonImagesCBox.Size = new System.Drawing.Size(300, 20);
        this.compressCompressedCBox.Location = new System.Drawing.Point(10, 210);
        this.compressCompressedCBox.Text = "Compress already compressed";
        this.compressCompressedCBox.Size = new System.Drawing.Size(300, 20);
        this.copySkippedCBox.Location = new System.Drawing.Point(10, 235);
        this.copySkippedCBox.Text = "Copy skipped images (as-is) to output";
        this.copySkippedCBox.Size = new System.Drawing.Size(300, 20);

        this.groupBox1.Controls.Add(this.stripMetadataCBox);
        this.groupBox1.Controls.Add(this.copyNonImagesCBox);
        this.groupBox1.Controls.Add(this.compressCompressedCBox);
        this.groupBox1.Controls.Add(this.copySkippedCBox);

        this.overwritePolicyLabel.Location = new System.Drawing.Point(10, 280);
        this.overwritePolicyLabel.Text = "Overwrite Policy:";
        this.overwritePolicyLabel.Size = new System.Drawing.Size(100, 20);
        this.overwritePolicyCombo.Location = new System.Drawing.Point(115, 277);
        this.overwritePolicyCombo.Size = new System.Drawing.Size(200, 21);
        this.overwritePolicyCombo.Items.AddRange(new object[] { "Append Suffix (1)", "Overwrite Original", "Skip Existing", "Overwrite Target" });
        
        this.groupBox1.Controls.Add(this.overwritePolicyLabel);
        this.groupBox1.Controls.Add(this.overwritePolicyCombo);

        this.formatPanel.Location = new System.Drawing.Point(10, 315);
        this.formatPanel.Size = new System.Drawing.Size(440, 40);
        this.saveJPEGRButton.Location = new System.Drawing.Point(0, 5);
        this.saveJPEGRButton.Text = "JPEG";
        this.saveJPEGRButton.Size = new System.Drawing.Size(120, 24);
        this.savePNGRButton.Location = new System.Drawing.Point(130, 5);
        this.savePNGRButton.Text = "PNG";
        this.savePNGRButton.Size = new System.Drawing.Size(120, 24);
        this.saveWEBPRButton.Location = new System.Drawing.Point(260, 5);
        this.saveWEBPRButton.Text = "WEBP";
        this.saveWEBPRButton.Size = new System.Drawing.Size(120, 24);
        this.formatPanel.Controls.Add(this.saveJPEGRButton);
        this.formatPanel.Controls.Add(this.savePNGRButton);
        this.formatPanel.Controls.Add(this.saveWEBPRButton);

        this.groupBox1.Controls.Add(this.formatPanel);

        // Skip Logic
        this.skipMinSizeCBox = new System.Windows.Forms.CheckBox();
        this.minSizeNumericKB = new System.Windows.Forms.NumericUpDown();
        this.skipMinResCBox = new System.Windows.Forms.CheckBox();
        this.minWidthNumeric = new System.Windows.Forms.NumericUpDown();
        this.minHeightNumeric = new System.Windows.Forms.NumericUpDown();

        this.skipMinSizeCBox.Location = new System.Drawing.Point(10, 365);
        this.skipMinSizeCBox.Text = "Skip if size smaller than (KB):";
        this.skipMinSizeCBox.Size = new System.Drawing.Size(180, 20);
        this.minSizeNumericKB.Location = new System.Drawing.Point(195, 363);
        this.minSizeNumericKB.Size = new System.Drawing.Size(60, 20);
        this.minSizeNumericKB.Maximum = 999999;
 
        this.skipMinResCBox.Location = new System.Drawing.Point(10, 395);
        this.skipMinResCBox.Text = "Skip if res smaller than:";
        this.skipMinResCBox.Size = new System.Drawing.Size(150, 20);
        this.minWidthNumeric.Location = new System.Drawing.Point(165, 393);
        this.minWidthNumeric.Size = new System.Drawing.Size(60, 20);
        this.minWidthNumeric.Maximum = 99999;
        Label xLabel = new Label() { Text = "x", Location = new System.Drawing.Point(232, 395), Size = new System.Drawing.Size(15, 20) };
        this.minHeightNumeric.Location = new System.Drawing.Point(250, 393);
        this.minHeightNumeric.Size = new System.Drawing.Size(60, 20);
        this.minHeightNumeric.Maximum = 99999;

        this.groupBox1.Controls.Add(this.skipMinSizeCBox);
        this.groupBox1.Controls.Add(this.minSizeNumericKB);
        this.groupBox1.Controls.Add(this.skipMinResCBox);
        this.groupBox1.Controls.Add(this.minWidthNumeric);
        this.groupBox1.Controls.Add(xLabel);
        this.groupBox1.Controls.Add(this.minHeightNumeric);

        this.groupBox2.Location = new System.Drawing.Point(10, 570);
        this.groupBox2.Size = new System.Drawing.Size(460, 65);
        this.groupBox2.Text = "Output Directory";
        this.savePathInput.Location = new System.Drawing.Point(10, 25);
        this.savePathInput.Size = new System.Drawing.Size(400, 20);
        this.chooseSaveFolderBtn.Location = new System.Drawing.Point(415, 23);
        this.chooseSaveFolderBtn.Size = new System.Drawing.Size(35, 23);
        
        this.groupBox2.Controls.Add(this.savePathInput);
        this.groupBox2.Controls.Add(this.chooseSaveFolderBtn);

        // Buttons
        this.dryRunBtn.Location = new System.Drawing.Point(220, 645);
        this.dryRunBtn.Size = new System.Drawing.Size(120, 40);
        this.dryRunBtn.Text = "Dry Run (Stats)";
        this.compressBtn.Location = new System.Drawing.Point(350, 645);
        this.compressBtn.Size = new System.Drawing.Size(120, 40);
        this.compressBtn.Text = "START";
        this.compressBtn.BackColor = System.Drawing.Color.LightGreen;

        // Status Strip
        this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { 
            this.statusLabel, 
            new System.Windows.Forms.ToolStripStatusLabel { Spring = true }, // Spacer
            this.versionLabel,
            this.aboutLink 
        });
        this.statusLabel.Text = "Ready";
        this.versionLabel.Text = "";
        this.versionLabel.ForeColor = System.Drawing.Color.Gray;
        this.aboutLink.Text = "About";
        this.aboutLink.IsLink = true;
        this.aboutLink.Click += new System.EventHandler(this.aboutLink_Click);

        // MainForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1100, 740);
        this.Controls.Add(this.splitContainer);
        this.Controls.Add(this.statusStrip);
        this.Name = "MainForm";
        this.Text = "LilPic";

        this.splitContainer.Panel1.ResumeLayout(false);
        this.splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
        this.splitContainer.ResumeLayout(false);
        this.directoryBox.ResumeLayout(false);
        this.directoryBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.qualityNumeric)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.qualityTrackBar)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.resizeNumeric)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.resizeTrackBar)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.targetWidthInput)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.targetHeightInput)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.minSizeNumericKB)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.minWidthNumeric)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.minHeightNumeric)).EndInit();
        this.groupBox2.ResumeLayout(false);
        this.groupBox2.PerformLayout();
        this.statusStrip.ResumeLayout(false);
        this.statusStrip.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.SplitContainer splitContainer;
    private LilPic.UI.FileTreePanel fileTreePanel;
    private System.Windows.Forms.Panel settingsPanel;
    private System.Windows.Forms.Label presetsLabel;
    private System.Windows.Forms.ComboBox presetsCombo;
    private System.Windows.Forms.GroupBox directoryBox;
    private System.Windows.Forms.TextBox directoryPathInput;
    private System.Windows.Forms.Button chooseFolderBtn;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label qualityLabel;
    private System.Windows.Forms.TrackBar qualityTrackBar;
    private System.Windows.Forms.NumericUpDown qualityNumeric;
    private System.Windows.Forms.ComboBox resizeModeCombo;
    private System.Windows.Forms.Label resizeLabel;
    private System.Windows.Forms.TrackBar resizeTrackBar;
    private System.Windows.Forms.NumericUpDown resizeNumeric;
    private System.Windows.Forms.CheckBox stripMetadataCBox;
    private System.Windows.Forms.CheckBox copyNonImagesCBox;
    private System.Windows.Forms.CheckBox compressCompressedCBox;
    private System.Windows.Forms.CheckBox copySkippedCBox;
    private System.Windows.Forms.Label overwritePolicyLabel;
    private System.Windows.Forms.ComboBox overwritePolicyCombo;
    private System.Windows.Forms.Panel formatPanel;
    private System.Windows.Forms.RadioButton saveWEBPRButton;
    private System.Windows.Forms.RadioButton savePNGRButton;
    private System.Windows.Forms.RadioButton saveJPEGRButton;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.TextBox savePathInput;
    private System.Windows.Forms.Button chooseSaveFolderBtn;
    private System.Windows.Forms.Button compressBtn;
    private System.Windows.Forms.Button dryRunBtn;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    private System.Windows.Forms.ToolStripStatusLabel versionLabel;
    private System.Windows.Forms.ToolStripStatusLabel aboutLink;

    // Additional target panels needed for logic
    private System.Windows.Forms.Panel targetSizePanel;
    private System.Windows.Forms.Label labelTargetSize;
    private System.Windows.Forms.TextBox targetSizeInput;
    private System.Windows.Forms.Panel targetResPanel;
    private System.Windows.Forms.Label labelW;
    private System.Windows.Forms.Label labelH;
    private System.Windows.Forms.NumericUpDown targetWidthInput;
    private System.Windows.Forms.NumericUpDown targetHeightInput;
    private System.Windows.Forms.CheckBox skipMinSizeCBox;
    private System.Windows.Forms.NumericUpDown minSizeNumericKB;
    private System.Windows.Forms.CheckBox skipMinResCBox;
    private System.Windows.Forms.NumericUpDown minWidthNumeric;
    private System.Windows.Forms.NumericUpDown minHeightNumeric;
}