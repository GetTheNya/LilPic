using System.ComponentModel;

namespace LilPic.UI; 

partial class ProcessDialog {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

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
        this.progressBar1 = new System.Windows.Forms.ProgressBar();
        this.button1 = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.logBox = new System.Windows.Forms.RichTextBox();
        this.workerPanel = new System.Windows.Forms.FlowLayoutPanel();
        this.SuspendLayout();
        // 
        // progressBar1
        // 
        this.progressBar1.Location = new System.Drawing.Point(12, 33);
        this.progressBar1.Name = "progressBar1";
        this.progressBar1.Size = new System.Drawing.Size(776, 23);
        this.progressBar1.TabIndex = 0;
        // 
        // button1
        // 
        this.button1.Location = new System.Drawing.Point(676, 444);
        this.button1.Name = "button1";
        this.button1.Size = new System.Drawing.Size(112, 38);
        this.button1.TabIndex = 1;
        this.button1.Text = "Cancel";
        this.button1.UseVisualStyleBackColor = true;
        this.button1.Click += new System.EventHandler(this.button1_Click);
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(12, 7);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(776, 23);
        this.label1.TabIndex = 2;
        this.label1.Text = "Calculating...";
        // 
        // logBox
        // 
        this.logBox.Location = new System.Drawing.Point(12, 180);
        this.logBox.Name = "logBox";
        this.logBox.ReadOnly = true;
        this.logBox.Size = new System.Drawing.Size(776, 258);
        this.logBox.TabIndex = 3;
        this.logBox.Text = "";
        // 
        // workerPanel
        // 
        this.workerPanel.Location = new System.Drawing.Point(12, 65);
        this.workerPanel.Name = "workerPanel";
        this.workerPanel.Size = new System.Drawing.Size(776, 109);
        this.workerPanel.TabIndex = 4;
        this.workerPanel.AutoScroll = true;

        // Initialize Worker Labels
        int cores = System.Environment.ProcessorCount;
        this.workerLabels = new System.Windows.Forms.Label[cores];
        for (int i = 0; i < cores; i++) {
            this.workerLabels[i] = new System.Windows.Forms.Label {
                Text = "[Idle]",
                Size = new System.Drawing.Size(180, 20),
                ForeColor = System.Drawing.Color.Gray,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.workerPanel.Controls.Add(this.workerLabels[i]);
        }

        // 
        // ProcessDialog
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 494);
        this.Controls.Add(this.workerPanel);
        this.Controls.Add(this.logBox);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.button1);
        this.Controls.Add(this.progressBar1);
        this.Name = "ProcessDialog";
        this.Text = "Compression Process";
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Label label1;

    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.Button button1;

    #endregion
}