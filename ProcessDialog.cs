using System;
using System.Windows.Forms;

namespace BulkImageCompressor;

public partial class ProcessDialog : Form {
    private bool CanClose = false;
    public Action CancelAction { get; set; }

    public ProcessDialog() {
        InitializeComponent();
    }

    public void UpdateProgress(int completed, int allCount) {
        if (IsDisposed || !IsHandleCreated) return;
        int percentage = (completed * 100) / allCount;
        Invoke(new MethodInvoker(() => { progressBar1.Value = percentage; }));
        Invoke(new MethodInvoker(() => { label1.Text = $"Completed {completed} of {allCount}"; }));
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
