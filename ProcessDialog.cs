using System;
using System.Drawing;
using System.Windows.Forms;

namespace BulkImageCompressor;

public partial class ProcessDialog : Form {
    private bool CanClose = false;
    public Action CancelAction { get; set; }

    private Label[] workerLabels;
    private RichTextBox logBox;
    private FlowLayoutPanel workerPanel;

    public bool IsDryRun { get; set; }

    public ProcessDialog() {
        InitializeComponent();
    }

    public void UpdateProgress(int completed, int allCount) {
        if (IsDisposed || !IsHandleCreated) return;
        int percentage = allCount > 0 ? (completed * 100) / allCount : 0;
        
        // Use BeginInvoke for non-blocking UI updates
        BeginInvoke(new MethodInvoker(() => { 
            if (IsDisposed) return;
            progressBar1.Value = percentage;
            string prefix = IsDryRun ? "Estimating" : "Completed";
            label1.Text = $"{prefix} {completed} of {allCount}"; 
            if (IsDryRun) this.Text = "Dry Run - Analyzing Images...";
        }));
    }

    public void UpdateWorkerSlot(int slot, string fileName) {
        if (IsDisposed || !IsHandleCreated || slot < 0 || slot >= (workerLabels?.Length ?? 0)) return;
        
        BeginInvoke(new MethodInvoker(() => {
            if (IsDisposed || slot >= (workerLabels?.Length ?? 0)) return;
            workerLabels[slot].Text = string.IsNullOrEmpty(fileName) ? "[Idle]" : $"[{slot + 1}] {fileName}";
            workerLabels[slot].ForeColor = string.IsNullOrEmpty(fileName) ? Color.Gray : Color.Black;
        }));
    }

    public void Log(string message, bool isError = false) {
        if (IsDisposed || !IsHandleCreated) return;
        
        BeginInvoke(new MethodInvoker(() => {
            if (IsDisposed) return;
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;
            logBox.SelectionColor = isError ? Color.Red : Color.Black;
            logBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            logBox.ScrollToCaret();
        }));
    }

    public void ProcessCompleted() {
        if (IsDisposed || !IsHandleCreated) return;
        
        BeginInvoke(new MethodInvoker(() => {
            if (IsDisposed) return;
            CanClose = true;
            Close();
        }));
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);

        if (e.CloseReason == CloseReason.UserClosing && !CanClose) {
            e.Cancel = true;
            // Trigger cancel action if user tries to close manually
            button1_Click(this, EventArgs.Empty);
        }
    }

    private void button1_Click(object sender, EventArgs e) {
        CancelAction?.Invoke();
        button1.Enabled = false;
        button1.Text = "Canceling...";
    }
}
