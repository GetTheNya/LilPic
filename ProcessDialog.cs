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

    public ProcessDialog() {
        InitializeComponent();
    }

    public void UpdateProgress(int completed, int allCount) {
        if (IsDisposed || !IsHandleCreated) return;
        int percentage = allCount > 0 ? (completed * 100) / allCount : 0;
        Invoke(new MethodInvoker(() => { progressBar1.Value = percentage; }));
        Invoke(new MethodInvoker(() => { label1.Text = $"Completed {completed} of {allCount}"; }));
    }

    public void UpdateWorkerSlot(int slot, string fileName) {
        if (IsDisposed || !IsHandleCreated || slot < 0 || slot >= workerLabels.Length) return;
        Invoke(new MethodInvoker(() => {
            workerLabels[slot].Text = string.IsNullOrEmpty(fileName) ? "[Idle]" : $"[{slot + 1}] {fileName}";
            workerLabels[slot].ForeColor = string.IsNullOrEmpty(fileName) ? Color.Gray : Color.Black;
        }));
    }

    public void Log(string message, bool isError = false) {
        if (IsDisposed || !IsHandleCreated) return;
        Invoke(new MethodInvoker(() => {
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;
            logBox.SelectionColor = isError ? Color.Red : Color.Black;
            logBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            logBox.ScrollToCaret();
        }));
    }

    public void ProcessCompleted() {
        if (IsDisposed || !IsHandleCreated) return;
        Invoke(new MethodInvoker(() => {
            CanClose = true;
            Close();
        }));
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);

        if (e.CloseReason == CloseReason.UserClosing && !CanClose) {
            e.Cancel = true;
        }
    }

    private void button1_Click(object sender, EventArgs e) {
        CancelAction?.Invoke();
        button1.Enabled = false;
        button1.Text = "Canceling...";
    }
}
