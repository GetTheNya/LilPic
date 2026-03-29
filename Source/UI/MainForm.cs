using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;

using LilPic.Models;
using LilPic.Services;
using LilPic.Utils;

namespace LilPic.UI;

public partial class MainForm : Form {
    private AppSettings settings;
    private Compressor compressor;
    private bool suppressSettingsUpdate = false;
    private bool isManualSavePath = false;

    public MainForm() {
        InitializeComponent();
        this.Icon = CommonUtils.AppIcon;
        this.statusLabel.Text = "Ready";
        this.versionLabel.Text = GetAppVersion();
        this.versionLabel.ForeColor = System.Drawing.Color.Gray;
        LoadSettings();
        SetupEvents();
        
        // Initial state
        UpdateResizeModeUI();
    }

    private string GetAppVersion() {
        var version = Application.ProductVersion;
        // Strip out the git commit hash if present (+...)
        int plusIndex = version.IndexOf('+');
        if (plusIndex >= 0) version = version.Substring(0, plusIndex);

        // Trim build/revision if it's .0.0.0 from local build
        if (version.EndsWith(".0")) version = version.Substring(0, version.Length - 2);
        if (version.EndsWith(".0")) version = version.Substring(0, version.Length - 2);
        if (version == "0.0.0" || version == "1.0.0.0") version = "v0.0.0-dev";
        
        if (!version.StartsWith("v") && version != "v0.0.0-dev") version = "v" + version;
        return version;
    }

    private void LoadSettings() {
        settings = AppSettings.Load();
        suppressSettingsUpdate = true;

        directoryPathInput.Text = settings.LastInputFolder;
        savePathInput.Text = settings.LastOutputFolder;
        
        qualityTrackBar.Minimum = 0;
        qualityTrackBar.Maximum = 100;
        qualityTrackBar.Value = Math.Clamp(settings.Quality, 0, 100);
        qualityNumeric.Value = Math.Clamp(settings.Quality, 0, 100);

        resizeTrackBar.Minimum = 0;
        resizeTrackBar.Maximum = 100;
        resizeTrackBar.Value = Math.Clamp(settings.ResizePercent, 0, 100);
        resizeNumeric.Value = Math.Clamp(settings.ResizePercent, 0, 100);
        
        resizeModeCombo.SelectedIndex = Math.Clamp(settings.ResizeMode, 0, 2);
        overwritePolicyCombo.SelectedIndex = settings.OverwritePolicy;
        stripMetadataCBox.Checked = settings.StripMetadata;
        copyNonImagesCBox.Checked = settings.CopyNonImages;
        copySkippedCBox.Checked = settings.CopySkippedImages;
        compressCompressedCBox.Checked = settings.CompressAlreadyCompressed;
        switch (settings.SaveAsFormat) {
            case 0: saveJPEGRButton.Checked = true; break;
            case 1: savePNGRButton.Checked = true; break;
            case 2: saveWEBPRButton.Checked = true; break;
        }

        skipMinSizeCBox.Checked = settings.SkipIfSmallerThanSize;
        minSizeNumericKB.Value = (decimal)settings.MinSizeToProcessKB;
        skipMinResCBox.Checked = settings.SkipIfSmallerThanRes;
        minWidthNumeric.Value = settings.MinWidthToProcess;
        minHeightNumeric.Value = settings.MinHeightToProcess;

        suppressSettingsUpdate = false;
        
        if (!string.IsNullOrEmpty(settings.LastInputFolder)) {
            fileTreePanel.LoadDirectory(settings.LastInputFolder);
        }
    }

    private void SaveSettings() {
        if (suppressSettingsUpdate) return;
        settings.LastInputFolder = directoryPathInput.Text;
        settings.LastOutputFolder = savePathInput.Text;
        settings.Quality = (int)qualityNumeric.Value;
        settings.ResizePercent = (int)resizeNumeric.Value;
        settings.ResizeMode = resizeModeCombo.SelectedIndex;
        settings.OverwritePolicy = overwritePolicyCombo.SelectedIndex;
        settings.StripMetadata = stripMetadataCBox.Checked;
        settings.CopyNonImages = copyNonImagesCBox.Checked;
        settings.CopySkippedImages = copySkippedCBox.Checked;
        settings.CompressAlreadyCompressed = compressCompressedCBox.Checked;
        
        settings.TargetWidth = (int)targetWidthInput.Value;
        settings.TargetHeight = (int)targetHeightInput.Value;
        if (long.TryParse(targetSizeInput.Text, out long kb)) settings.TargetFileSizeKB = kb;

        if (saveJPEGRButton.Checked) settings.SaveAsFormat = 0;
        else if (savePNGRButton.Checked) settings.SaveAsFormat = 1;
        else if (saveWEBPRButton.Checked) settings.SaveAsFormat = 2;

        settings.SkipIfSmallerThanSize = skipMinSizeCBox.Checked;
        settings.MinSizeToProcessKB = (long)minSizeNumericKB.Value;
        settings.SkipIfSmallerThanRes = skipMinResCBox.Checked;
        settings.MinWidthToProcess = (int)minWidthNumeric.Value;
        settings.MinHeightToProcess = (int)minHeightNumeric.Value;

        fileTreePanel.SetSkipFilters(
            settings.SkipIfSmallerThanSize, settings.MinSizeToProcessKB,
            settings.SkipIfSmallerThanRes, settings.MinWidthToProcess, settings.MinHeightToProcess);

        settings.Save();
    }

    private void SetupEvents() {
        presetsCombo.SelectedIndexChanged += PresetsCombo_SelectedIndexChanged;
        resizeModeCombo.SelectedIndexChanged += (s, e) => { UpdateResizeModeUI(); SaveSettings(); };
        
        qualityTrackBar.Scroll += (s, e) => { 
            if (qualityNumeric.Value != qualityTrackBar.Value) {
                qualityNumeric.Value = qualityTrackBar.Value; 
                SaveSettings(); 
            }
        };
        qualityNumeric.ValueChanged += (s, e) => { 
            if (qualityTrackBar.Value != (int)qualityNumeric.Value) {
                qualityTrackBar.Value = (int)qualityNumeric.Value; 
                SaveSettings(); 
            }
        };
        qualityNumeric.KeyUp += (s, e) => {
            if (decimal.TryParse(qualityNumeric.Text, out decimal val)) {
                if (val >= 0 && val <= 100) {
                    qualityNumeric.Value = val;
                    qualityTrackBar.Value = (int)val;
                    SaveSettings();
                }
            }
        };
        
        resizeTrackBar.Scroll += (s, e) => { 
            if (resizeNumeric.Value != resizeTrackBar.Value) {
                resizeNumeric.Value = resizeTrackBar.Value; 
                SaveSettings(); 
            }
        };
        resizeNumeric.ValueChanged += (s, e) => { 
            if (resizeTrackBar.Value != (int)resizeNumeric.Value) {
                resizeTrackBar.Value = (int)resizeNumeric.Value; 
                SaveSettings(); 
            }
        };
        resizeNumeric.KeyUp += (s, e) => {
            if (decimal.TryParse(resizeNumeric.Text, out decimal val)) {
                if (val >= 0 && val <= 100) {
                    resizeNumeric.Value = val;
                    resizeTrackBar.Value = (int)val;
                    SaveSettings();
                }
            }
        };

        directoryPathInput.TextChanged += (s, e) => { 
            string input = directoryPathInput.Text;
            if (Directory.Exists(input)) {
                fileTreePanel.LoadDirectory(input);
                if (!isManualSavePath && !string.IsNullOrEmpty(input)) {
                    savePathInput.Text = Path.Combine(input, "Compressed");
                }
            }
            SaveSettings(); 
        };
        
        savePathInput.TextChanged += (s, e) => {
            if (savePathInput.Focused) isManualSavePath = true;
            SaveSettings();
        };
        
        stripMetadataCBox.CheckedChanged += (s, e) => SaveSettings();
        copyNonImagesCBox.CheckedChanged += (s, e) => SaveSettings();
        copySkippedCBox.CheckedChanged += (s, e) => SaveSettings();
        compressCompressedCBox.CheckedChanged += (s, e) => SaveSettings();
        overwritePolicyCombo.SelectedIndexChanged += (s, e) => SaveSettings();
        
        saveJPEGRButton.CheckedChanged += (s, e) => SaveSettings();
        savePNGRButton.CheckedChanged += (s, e) => SaveSettings();
        saveWEBPRButton.CheckedChanged += (s, e) => SaveSettings();

        targetWidthInput.ValueChanged += (s, e) => SaveSettings();
        targetHeightInput.ValueChanged += (s, e) => SaveSettings();
        targetSizeInput.TextChanged += (s, e) => SaveSettings();

        skipMinSizeCBox.CheckedChanged += (s, e) => SaveSettings();
        minSizeNumericKB.KeyUp += (s, e) => {
            if (decimal.TryParse(minSizeNumericKB.Text, out decimal val)) {
                if (val >= minSizeNumericKB.Minimum && val <= minSizeNumericKB.Maximum) minSizeNumericKB.Value = val;
            }
            SaveSettings();
        };

        skipMinResCBox.CheckedChanged += (s, e) => SaveSettings();
        minWidthNumeric.ValueChanged += (s, e) => SaveSettings();
        minWidthNumeric.KeyUp += (s, e) => {
            if (decimal.TryParse(minWidthNumeric.Text, out decimal val)) {
                if (val >= minWidthNumeric.Minimum && val <= minWidthNumeric.Maximum) minWidthNumeric.Value = val;
            }
            SaveSettings();
        };
        minHeightNumeric.ValueChanged += (s, e) => SaveSettings();
        minHeightNumeric.KeyUp += (s, e) => {
            if (decimal.TryParse(minHeightNumeric.Text, out decimal val)) {
                if (val >= minHeightNumeric.Minimum && val <= minHeightNumeric.Maximum) minHeightNumeric.Value = val;
            }
            SaveSettings();
        };

        chooseFolderBtn.Click += chooseFolderBtn_Click;
        chooseSaveFolderBtn.Click += chooseSaveFolderBtn_Click;
        dryRunBtn.Click += dryRunBtn_Click;
        compressBtn.Click += compressBtn_Click;

        fileTreePanel.FileDoubleClicked += (s, filePath) => {
            if (fileTreePanel.IsCalculatingResolutions) {
                MessageBox.Show("Please wait, image resolutions are still being calculated.", "Resolution Calculating", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var format = saveJPEGRButton.Checked ? SKEncodedImageFormat.Jpeg :
                         savePNGRButton.Checked ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Webp;
            
            int targetW = resizeModeCombo.SelectedIndex == 1 ? (int)targetWidthInput.Value : 0;
            int targetH = resizeModeCombo.SelectedIndex == 1 ? (int)targetHeightInput.Value : 0;
            long targetSizeBytes = resizeModeCombo.SelectedIndex == 2 ? settings.TargetFileSizeKB * 1024 : 0;

            var preview = new PreviewWindow(filePath, (int)qualityNumeric.Value, (int)resizeNumeric.Value, 
                                          format, stripMetadataCBox.Checked, targetW, targetH, targetSizeBytes);
            preview.ShowDialog();
        };
    }

    private void UpdateResizeModeUI() {
        int mode = resizeModeCombo.SelectedIndex;
        resizeTrackBar.Visible = resizeLabel.Visible = resizeNumeric.Visible = (mode == 0);
        targetResPanel.Visible = (mode == 1);
        targetSizePanel.Visible = (mode == 2);

        if (mode == 1) {
            targetWidthInput.Value = settings.TargetWidth;
            targetHeightInput.Value = settings.TargetHeight;
        } else if (mode == 2) {
            targetSizeInput.Text = settings.TargetFileSizeKB.ToString();
        }
    }

    private void PresetsCombo_SelectedIndexChanged(object sender, EventArgs e) {
        suppressSettingsUpdate = true;
        switch (presetsCombo.SelectedIndex) {
            case 1: // Web Optimized
                resizeModeCombo.SelectedIndex = 0;
                qualityNumeric.Value = 75;
                resizeNumeric.Value = 80;
                saveJPEGRButton.Checked = true;
                stripMetadataCBox.Checked = true;
                break;
            case 2: // High Quality (PNG)
                resizeModeCombo.SelectedIndex = 0;
                qualityNumeric.Value = 90;
                resizeNumeric.Value = 100;
                savePNGRButton.Checked = true;
                stripMetadataCBox.Checked = false;
                break;
            case 3: // Max Compression (WEBP)
                resizeModeCombo.SelectedIndex = 0;
                qualityNumeric.Value = 60;
                resizeNumeric.Value = 70;
                saveWEBPRButton.Checked = true;
                stripMetadataCBox.Checked = true;
                break;
            case 4: // Social Media (Small)
                resizeModeCombo.SelectedIndex = 1; // Target Res
                targetWidthInput.Value = 1080;
                targetHeightInput.Value = 1080;
                qualityNumeric.Value = 80;
                saveJPEGRButton.Checked = true;
                stripMetadataCBox.Checked = true;
                break;
        }
        UpdateResizeModeUI();
        suppressSettingsUpdate = false;
        SaveSettings();
    }

    private void chooseFolderBtn_Click(object sender, EventArgs e) {
        using var fbd = new FolderBrowserDialog();
        if (fbd.ShowDialog() == DialogResult.OK) {
            directoryPathInput.Text = fbd.SelectedPath;
        }
    }

    private void chooseSaveFolderBtn_Click(object sender, EventArgs e) {
        using var fbd = new FolderBrowserDialog();
        if (fbd.ShowDialog() == DialogResult.OK) {
            savePathInput.Text = fbd.SelectedPath;
        }
    }

    private void dryRunBtn_Click(object sender, EventArgs e) {
        var selectedFiles = fileTreePanel.GetSelectedFiles();
        if (selectedFiles.Count == 0) {
            MessageBox.Show("Please select files in the explorer first.");
            return;
        }

        SaveAs format = saveJPEGRButton.Checked ? SaveAs.JPEG : 
                        savePNGRButton.Checked ? SaveAs.PNG : SaveAs.WEBP;

        var dryRunCompressor = new Compressor(directoryPathInput.Text) {
            SavePath = savePathInput.Text, // Not used but needed for constructor logic
            Quality = (int)qualityNumeric.Value,
            Resize = (int)resizeNumeric.Value,
            SaveAs = format,
            StripMetadata = stripMetadataCBox.Checked,
            CopyNonImages = copyNonImagesCBox.Checked,
            CopySkippedImages = copySkippedCBox.Checked,
            OverwritePolicy = overwritePolicyCombo.SelectedIndex,
            ExplicitFiles = selectedFiles,
            TargetWidth = settings.ResizeMode == 1 ? (int)targetWidthInput.Value : 0,
            TargetHeight = settings.ResizeMode == 1 ? (int)targetHeightInput.Value : 0,
            TargetFileSizeBytes = settings.ResizeMode == 2 ? settings.TargetFileSizeKB * 1024 : 0,
            SkipIfSmallerThanSize = settings.SkipIfSmallerThanSize,
            MinSizeToProcessKB = settings.MinSizeToProcessKB,
            SkipIfSmallerThanRes = settings.SkipIfSmallerThanRes,
            MinWidthToProcess = settings.MinWidthToProcess,
            MinHeightToProcess = settings.MinHeightToProcess,
            CompressChildren = true,
            IsDryRun = true
        };

        var dialog = new ProcessDialog { IsDryRun = true };
        dialog.CancelAction = () => dryRunCompressor.Cancel();

        dryRunCompressor.ProcessEvent += (s, args) => dialog.UpdateProgress(args.ProcessedFiles, args.AllFiles);
        dryRunCompressor.WorkerActivity += (s, act) => dialog.UpdateWorkerSlot(act.Slot, act.FileName);
        dryRunCompressor.LogMessage += (s, msg) => dialog.Log(msg.Message, msg.IsError);
        dryRunCompressor.FileProcessed += (s, arg) => {
            fileTreePanel.UpdateNodeStatus(arg.FilePath, arg.Status, arg.Reason, arg.EstimatedSize);
        };
        
        dryRunCompressor.ProcessCompleted += (s, args) => {
            dialog.ProcessCompleted();
            fileTreePanel.RefreshList();
            this.Invoke(new MethodInvoker(() => {
                long originalSize = dryRunCompressor.TotalOriginalSize;
                long estimatedSize = dryRunCompressor.TotalEstimatedSize;
                long savings = Math.Max(0, originalSize - estimatedSize);
                double percent = originalSize > 0 ? (savings * 100.0) / originalSize : 0;

                MessageBox.Show(
                    $"Dry Run Finished!\n\n" +
                    $"Images Processed: {args.ProcessedFiles}\n" +
                    $"Original Size: {CommonUtils.FormatSize(originalSize)}\n" +
                    $"Estimated Size: {CommonUtils.FormatSize(estimatedSize)}\n" +
                    $"Potential Savings: {CommonUtils.FormatSize(savings)} ({percent:F1}%)",
                    "Dry Run Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        };

        dryRunCompressor.ProcessCanceled += (s, args) => {
            dialog.ProcessCompleted();
            fileTreePanel.RefreshList();
            this.Invoke(new MethodInvoker(() => MessageBox.Show("Dry Run Canceled.")));
        };

        dryRunCompressor.CompressAsync();
        dialog.ShowDialog();
    }

    private void compressBtn_Click(object sender, EventArgs e) {
        var selectedFiles = fileTreePanel.GetSelectedFiles();
        if (selectedFiles.Count == 0) {
            MessageBox.Show("Please select files in the explorer first.");
            return;
        }

        if (string.IsNullOrEmpty(savePathInput.Text)) {
            MessageBox.Show("Please select a save directory.");
            return;
        }

        SaveAs format = saveJPEGRButton.Checked ? SaveAs.JPEG : 
                        savePNGRButton.Checked ? SaveAs.PNG : SaveAs.WEBP;

        compressor = new Compressor(directoryPathInput.Text) {
            SavePath = savePathInput.Text,
            Quality = (int)qualityNumeric.Value,
            Resize = (int)resizeNumeric.Value,
            SaveAs = format,
            StripMetadata = stripMetadataCBox.Checked,
            CopyNonImages = copyNonImagesCBox.Checked,
            CopySkippedImages = copySkippedCBox.Checked,
            CompressCompressed = compressCompressedCBox.Checked,
            OverwritePolicy = overwritePolicyCombo.SelectedIndex,
            ExplicitFiles = selectedFiles,
            TargetWidth = settings.ResizeMode == 1 ? (int)targetWidthInput.Value : 0,
            TargetHeight = settings.ResizeMode == 1 ? (int)targetHeightInput.Value : 0,
            TargetFileSizeBytes = settings.ResizeMode == 2 ? settings.TargetFileSizeKB * 1024 : 0,
            SkipIfSmallerThanSize = settings.SkipIfSmallerThanSize,
            MinSizeToProcessKB = settings.MinSizeToProcessKB,
            SkipIfSmallerThanRes = settings.SkipIfSmallerThanRes,
            MinWidthToProcess = settings.MinWidthToProcess,
            MinHeightToProcess = settings.MinHeightToProcess,
            CompressChildren = true
        };

        var dialog = new ProcessDialog();
        dialog.CancelAction = () => compressor.Cancel();

        compressor.ProcessEvent += (s, args) => dialog.UpdateProgress(args.ProcessedFiles, args.AllFiles);
        compressor.WorkerActivity += (s, act) => dialog.UpdateWorkerSlot(act.Slot, act.FileName);
        compressor.LogMessage += (s, msg) => dialog.Log(msg.Message, msg.IsError);
        compressor.FileProcessed += (s, arg) => {
            fileTreePanel.UpdateNodeStatus(arg.FilePath, arg.Status, arg.Reason, arg.EstimatedSize);
        };

        compressor.ProcessCompleted += (s, args) => {
            dialog.ProcessCompleted();
            fileTreePanel.RefreshList();
            this.Invoke(new MethodInvoker(() => MessageBox.Show("Compression Finished!")));
        };
        compressor.ProcessCanceled += (s, args) => {
            dialog.ProcessCompleted();
            fileTreePanel.RefreshList();
            this.Invoke(new MethodInvoker(() => MessageBox.Show("Compression Canceled.")));
        };

        compressor.CompressAsync();
        dialog.ShowDialog();
    }

    private void aboutLink_Click(object sender, EventArgs e) {
        string aboutTitle = "About LilPic";
        string aboutText = "LilPic - Bulk Image Compressor\n" +
                           "Version: " + GetAppVersion() + "\n\n" +
                           "A powerful and lightweight tool for bulk image compression and resizing.\n\n" +
                           "License: MIT\n" +
                           "Author: GetTheNya\n" +
                           "GitHub: github.com/GetTheNya/LilPic";

        MessageBox.Show(aboutText, aboutTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}