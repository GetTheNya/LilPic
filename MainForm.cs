using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BulkImageCompressor;

public partial class MainForm : Form {
    private readonly System.Windows.Forms.Timer _takeBackupBlinkTimer = new();
    private CheckBox stripMetadataCBox;

    public MainForm() {
        _takeBackupBlinkTimer.Interval = 500;
        _takeBackupBlinkTimer.Enabled = false;
        _takeBackupBlinkTimer.Tick += takeBackupBlinkTimer_Tick;

        InitializeComponent();

        KBMKBox.SelectedIndex = 0;
        AddStripMetadataCheckbox();
    }

    private void AddStripMetadataCheckbox() {
        stripMetadataCBox = new CheckBox {
            Text = "Strip EXIF Metadata (smaller files)",
            Location = new Point(447, 46 + 25),
            Size = new Size(250, 21),
            Checked = true
        };
        groupBox1.Controls.Add(stripMetadataCBox);
        
        // Move panel down a bit
        panel1.Location = new Point(panel1.Location.X, panel1.Location.Y + 20);
    }


    private void chooseFolderBtn_Click(object sender, EventArgs e) {
        using (var fbd = new FolderBrowserDialog()) {
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                directoryPathInput.Text = fbd.SelectedPath;
                savePathInput.Text = $"{fbd.SelectedPath}\\Compressed";
            }
        }
    }

    private void takeBackupBlinkTimer_Tick(object sender, EventArgs e) {
        backupLabel.ForeColor = backupLabel.ForeColor == Color.Red ? Color.Black : Color.Red;
    }


    private void overrideCBox_CheckedChanged(object sender, EventArgs e) {
        backupLabel.Visible = overrideCBox.Checked;
        _takeBackupBlinkTimer.Enabled = overrideCBox.Checked;
        savePathInput.Enabled = !overrideCBox.Checked;
        if (overrideCBox.Checked)
            savePathInput.Text = "Overwriting!";
        else
            savePathInput.Text = Directory.Exists(directoryPathInput.Text)
                ? $"{directoryPathInput.Text}\\Compressed"
                : "";
    }

    private void qualityTrackBar_Scroll(object sender, EventArgs e) {
        qualityNumeric.Value = qualityTrackBar.Value;
    }

    private void qualityNumeric_ValueChanged(object sender, EventArgs e) {
        qualityTrackBar.Value = (int)qualityNumeric.Value;
    }

    private void resizeTrackBar_Scroll(object sender, EventArgs e) {
        resizeNumeric.Value = resizeTrackBar.Value;
    }

    private void resizeNumeric_ValueChanged(object sender, EventArgs e) {
        resizeTrackBar.Value = (int)resizeNumeric.Value;
    }

    private void bigFileCBox_CheckedChanged(object sender, EventArgs e) {
        KBMKBox.Enabled = bigFileCBox.Checked;
        fileSizeInput.Enabled = bigFileCBox.Checked;
    }

    private void chooseSaveFolderBtn_Click(object sender, EventArgs e) {
        using (var fbd = new FolderBrowserDialog()) {
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                savePathInput.Text = fbd.SelectedPath;
            }
        }
    }

    private void compressBtn_Click(object sender, EventArgs e) {
        string compressPath = directoryPathInput.Text;

        if (string.IsNullOrWhiteSpace(compressPath) || !Directory.Exists(compressPath)) {
            MessageBox.Show("Please select a valid input folder.", "Error!");
            return;
        }

        string savePath = savePathInput.Text;
        int quality = qualityTrackBar.Value;
        int resize = resizeTrackBar.Value;
        bool compressChild = compressChildrenCBox.Checked;
        bool overwrite = overrideCBox.Checked;
        bool compressCompressed = compressCompressedCBox.Checked;
        SaveAs saveAs;
        int minimumFileSize = -1;


        var checkedButton = panel1.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
        saveAs = (SaveAs)checkedButton.TabIndex;

        if (bigFileCBox.Checked) {
            var parsed = int.TryParse(fileSizeInput.Text, out minimumFileSize);
            if (!parsed) {
                MessageBox.Show("Please check inputs", "Error!");
                return;
            }

            switch (KBMKBox.SelectedIndex) {
                case 0:
                    minimumFileSize *= 1024;
                    break;
                case 1:
                    minimumFileSize *= 1048576;
                    break;
                default: return;
            }
        }

        var compressor = new Compressor(compressPath, savePath, quality, resize, compressChild, overwrite, compressCompressed, saveAs,
            minimumFileSize);
        compressor.StripMetadata = stripMetadataCBox.Checked;

        var progressWin = new ProcessDialog();
        progressWin.CancelAction = () => compressor.Cancel();
        progressWin.Show(this);

        compressor.ProcessEvent += (o, a) => {
            progressWin.UpdateProgress(a.ProcessedFiles, a.AllFiles);
        };
        
        compressor.ProcessCompleted += (o, a) => {
            progressWin.ProcessCompleted();
            Invoke(new MethodInvoker(() => {
                MessageBox.Show(this, $"Process completed! {a.ProcessedFiles} of {a.AllFiles}", "Success");
            }));
        };

        compressor.ProcessCanceled += (o, a) => {
            progressWin.ProcessCompleted();
            Invoke(new MethodInvoker(() => {
                MessageBox.Show(this, $"Process canceled! {a.ProcessedFiles} of {a.AllFiles} processed.", "Canceled");
            }));
        };

        compressor.ErrorOccurred += (o, msg) => {
            Invoke(new MethodInvoker(() => {
                MessageBox.Show(this, msg, "Process Error");
            }));
        };
        
        compressor.CompressAsync();
        
    }

    private void directoryPathInput_TextChanged(object sender, EventArgs e) {
        savePathInput.Text = Directory.Exists(directoryPathInput.Text) ? $"{directoryPathInput.Text}\\Compressed" : "";
    }
}