using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BulkImageCompressor;

public partial class FileTreePanel : UserControl {
    private ListView fileListView;
    private TextBox searchBox;
    private CheckBox regexCBox;
    private FlowLayoutPanel filterPanel;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel countLabel;

    private List<FileNode> allNodes = new();
    private List<FileNode> filteredNodes = new();

    public event EventHandler SelectionChanged;

    public FileTreePanel() {
        InitializeComponent();
        SetupUI();
    }

    private void SetupUI() {
        this.Dock = DockStyle.Fill;

        // Search Bar
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
        searchBox = new TextBox { 
            Location = new Point(5, 10), 
            Size = new Size(200, 20),
            PlaceholderText = "Search files..."
        };
        searchBox.TextChanged += (s, e) => ApplyFilter();
        
        regexCBox = new CheckBox { 
            Text = "Regex", 
            Location = new Point(215, 10), 
            AutoSize = true 
        };
        regexCBox.CheckedChanged += (s, e) => ApplyFilter();

        topPanel.Controls.Add(searchBox);
        topPanel.Controls.Add(regexCBox);

        // Filter Chips
        filterPanel = new FlowLayoutPanel { 
            Dock = DockStyle.Top, 
            Height = 35, 
            Padding = new Padding(5)
        };
        AddFilterChip("All", true);
        AddFilterChip("> 5MB", false);
        AddFilterChip("PNG", false);
        AddFilterChip("JPG", false);
        AddFilterChip("WEBP", false);

        // List View
        fileListView = new ListView {
            Dock = DockStyle.Fill,
            View = View.Details,
            CheckBoxes = true,
            FullRowSelect = true,
            VirtualMode = true
        };
        fileListView.Columns.Add("Name", 250);
        fileListView.Columns.Add("Size", 100);
        fileListView.Columns.Add("Res", 120);
        fileListView.Columns.Add("Status", 100);

        fileListView.RetrieveVirtualItem += FileListView_RetrieveVirtualItem;
        fileListView.ItemCheck += FileListView_ItemCheck;
        fileListView.DoubleClick += FileListView_DoubleClick;

        // Status Strip
        statusStrip = new StatusStrip();
        countLabel = new ToolStripStatusLabel("0 files found");
        statusStrip.Items.Add(countLabel);

        this.Controls.Add(fileListView);
        this.Controls.Add(filterPanel);
        this.Controls.Add(topPanel);
        this.Controls.Add(statusStrip);
    }

    private void AddFilterChip(string text, bool active) {
        var btn = new Button { 
            Text = text, 
            FlatStyle = FlatStyle.Flat,
            BackColor = active ? Color.LightBlue : Color.White,
            AutoSize = true
        };
        btn.Click += (s, e) => {
            // Toggle logic or radio logic
            ApplyFilter();
        };
        filterPanel.Controls.Add(btn);
    }

    public void LoadDirectory(string path) {
        allNodes.Clear();
        if (!Directory.Exists(path)) return;

        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var f in files) {
            allNodes.Add(new FileNode(f));
        }
        ApplyFilter();
    }

    private void ApplyFilter() {
        string query = searchBox.Text.ToLower();
        filteredNodes = allNodes.Where(n => n.Name.ToLower().Contains(query)).ToList();
        
        fileListView.VirtualListSize = filteredNodes.Count;
        countLabel.Text = $"{filteredNodes.Count} files filtered ({allNodes.Count} total)";
        fileListView.Invalidate();
    }

    private void FileListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
        if (e.ItemIndex >= 0 && e.ItemIndex < filteredNodes.Count) {
            var node = filteredNodes[e.ItemIndex];
            var item = new ListViewItem(node.Name) { Checked = node.IsChecked };
            item.SubItems.Add(FormatSize(node.Size));
            item.SubItems.Add(node.Resolution);
            item.SubItems.Add(node.Status);

            if (node.Size > 10 * 1024 * 1024) item.ForeColor = Color.Red;
            else if (node.Size > 2 * 1024 * 1024) item.ForeColor = Color.Orange;

            e.Item = item;
        }
    }

    private void FileListView_ItemCheck(object sender, ItemCheckEventArgs e) {
        if (e.Index >= 0 && e.Index < filteredNodes.Count) {
            filteredNodes[e.Index].IsChecked = (e.NewValue == CheckState.Checked);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void FileListView_DoubleClick(object sender, EventArgs e) {
        if (fileListView.SelectedIndices.Count > 0) {
            var index = fileListView.SelectedIndices[0];
            var node = filteredNodes[index];
            FileDoubleClicked?.Invoke(this, node.Path);
        }
    }

    public event EventHandler<string> FileDoubleClicked;

    private string FormatSize(long bytes) {
        string[] units = { "B", "KB", "MB", "GB" };
        int unitIndex = 0;
        double size = bytes;
        while (size >= 1024 && unitIndex < units.Length - 1) {
            size /= 1024;
            unitIndex++;
        }
        return $"{size:F2} {units[unitIndex]}";
    }

    public List<string> GetSelectedFiles() => allNodes.Where(n => n.IsChecked).Select(n => n.Path).ToList();

    private void InitializeComponent() {
        this.SuspendLayout();
        this.Name = "FileTreePanel";
        this.Size = new Size(500, 600);
        this.ResumeLayout(false);
    }
}
