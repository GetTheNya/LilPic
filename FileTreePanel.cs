using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BulkImageCompressor;

public partial class FileTreePanel : UserControl {
    private ListView fileListView;
    private TreeView folderTreeView;
    private SplitContainer splitContainer;
    private TextBox searchBox;
    private CheckBox regexCBox;
    private FlowLayoutPanel filterPanel;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel countLabel;
    private CheckBox folderViewToggle;

    private List<FileNode> allNodes = new();
    private List<FileNode> filteredNodes = new();
    private string activeFilter = "All";
    private string currentRootPath = "";
    private string selectedFolderPath = "";

    public event EventHandler SelectionChanged;
    public event EventHandler<string> FileDoubleClicked;

    public FileTreePanel() {
        InitializeComponent();
        SetupUI();
    }

    private void SetupUI() {
        this.Dock = DockStyle.Fill;

        // Top Panel (Search & Toggle)
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
        searchBox = new TextBox { 
            Location = new Point(5, 10), 
            Size = new Size(180, 20),
            PlaceholderText = "Search files..."
        };
        searchBox.TextChanged += (s, e) => ApplyFilter();
        
        regexCBox = new CheckBox { 
            Text = "Regex", 
            Location = new Point(190, 10), 
            AutoSize = true 
        };
        regexCBox.CheckedChanged += (s, e) => ApplyFilter();

        folderViewToggle = new CheckBox {
            Text = "Folder Tree",
            Location = new Point(260, 10),
            AutoSize = true,
            Checked = false
        };
        folderViewToggle.CheckedChanged += (s, e) => {
            splitContainer.Panel1Collapsed = !folderViewToggle.Checked;
        };

        topPanel.Controls.Add(searchBox);
        topPanel.Controls.Add(regexCBox);
        topPanel.Controls.Add(folderViewToggle);

        // Filter Chips
        filterPanel = new FlowLayoutPanel { 
            Dock = DockStyle.Top, 
            Height = 35, 
            Padding = new Padding(5)
        };
        AddFilterChip("All");
        AddFilterChip("> 5MB");
        AddFilterChip("PNG");
        AddFilterChip("JPG");
        AddFilterChip("WEBP");

        // Split Container for Tree and List
        splitContainer = new SplitContainer {
            Dock = DockStyle.Fill,
            SplitterDistance = 180,
            Panel1Collapsed = true // Default to flat list
        };

        folderTreeView = new TreeView {
            Dock = DockStyle.Fill,
            FullRowSelect = true
        };
        folderTreeView.AfterSelect += (s, e) => {
            selectedFolderPath = e.Node.Tag as string;
            ApplyFilter();
        };

        // List View
        fileListView = new ListView {
            Dock = DockStyle.Fill,
            View = View.Details,
            CheckBoxes = true,
            FullRowSelect = true,
            VirtualMode = true
        };
        fileListView.Columns.Add("Name", 250);
        fileListView.Columns.Add("Size", 80);
        fileListView.Columns.Add("Res", 100);
        fileListView.Columns.Add("Status", 80);

        fileListView.RetrieveVirtualItem += FileListView_RetrieveVirtualItem;
        fileListView.ItemCheck += FileListView_ItemCheck;
        fileListView.DoubleClick += FileListView_DoubleClick;
        fileListView.MouseDown += FileListView_MouseDown;

        splitContainer.Panel1.Controls.Add(folderTreeView);
        splitContainer.Panel2.Controls.Add(fileListView);

        // Status Strip
        statusStrip = new StatusStrip();
        countLabel = new ToolStripStatusLabel("0 files found");
        statusStrip.Items.Add(countLabel);

        this.Controls.Add(splitContainer);
        this.Controls.Add(filterPanel);
        this.Controls.Add(topPanel);
        this.Controls.Add(statusStrip);
    }

    private void AddFilterChip(string text) {
        var btn = new Button { 
            Text = text, 
            FlatStyle = FlatStyle.Flat,
            BackColor = (text == activeFilter) ? Color.LightBlue : Color.White,
            AutoSize = true,
            Margin = new Padding(2)
        };
        btn.Click += (s, e) => {
            activeFilter = btn.Text;
            foreach (Control c in filterPanel.Controls) {
                if (c is Button b) b.BackColor = (b.Text == activeFilter) ? Color.LightBlue : Color.White;
            }
            ApplyFilter();
        };
        filterPanel.Controls.Add(btn);
    }

    public void LoadDirectory(string path) {
        if (!Directory.Exists(path)) return;
        currentRootPath = path;
        selectedFolderPath = "";
        
        allNodes.Clear();
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var f in files) {
            if (AllowedExtensions.IsImage(f)) {
                allNodes.Add(new FileNode(f));
            }
        }

        RebuildFolderTree(path);
        ApplyFilter();
    }

    private void RebuildFolderTree(string rootPath) {
        folderTreeView.Nodes.Clear();
        var rootNode = new TreeNode(Path.GetFileName(rootPath)) { Tag = rootPath };
        folderTreeView.Nodes.Add(rootNode);
        
        PopulateTree(rootPath, rootNode);
        rootNode.Expand();
    }

    private void PopulateTree(string path, TreeNode parentNode) {
        try {
            foreach (var dir in Directory.GetDirectories(path)) {
                var node = new TreeNode(Path.GetFileName(dir)) { Tag = dir };
                parentNode.Nodes.Add(node);
                PopulateTree(dir, node);
            }
        } catch { }
    }

    private void ApplyFilter() {
        IEnumerable<FileNode> query = allNodes;

        // Folder Filter
        if (folderViewToggle.Checked && !string.IsNullOrEmpty(selectedFolderPath)) {
            query = query.Where(n => n.Path.StartsWith(selectedFolderPath, StringComparison.OrdinalIgnoreCase));
        }

        // Search Filter
        string searchText = searchBox.Text.ToLower();
        if (!string.IsNullOrEmpty(searchText)) {
            if (regexCBox.Checked) {
                try {
                    var regex = new System.Text.RegularExpressions.Regex(searchText, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    query = query.Where(n => regex.IsMatch(n.Name));
                } catch { }
            } else {
                query = query.Where(n => n.Name.ToLower().Contains(searchText));
            }
        }

        // Chip Filter
        switch (activeFilter) {
            case "> 5MB":
                query = query.Where(n => n.Size > 5 * 1024 * 1024);
                break;
            case "PNG":
                query = query.Where(n => n.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
                break;
            case "JPG":
                query = query.Where(n => n.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || n.Name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));
                break;
            case "WEBP":
                query = query.Where(n => n.Name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase));
                break;
        }

        filteredNodes = query.ToList();
        fileListView.VirtualListSize = filteredNodes.Count;
        countLabel.Text = $"{filteredNodes.Count} files shown ({allNodes.Count} total images)";
        fileListView.Invalidate();
    }

    private void FileListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
        if (e.ItemIndex >= 0 && e.ItemIndex < filteredNodes.Count) {
            var node = filteredNodes[e.ItemIndex];
            var item = new ListViewItem(node.Name) { Checked = node.IsChecked };
            item.SubItems.Add(Utils.FormatSize(node.Size));
            item.SubItems.Add(node.Resolution);
            item.SubItems.Add(node.Status);

            if (node.Size > 10 * 1024 * 1024) item.ForeColor = Color.Red;
            else if (node.Size > 2 * 1024 * 1024) item.ForeColor = Color.Orange;

            e.Item = item;
        }
    }

    private void FileListView_ItemCheck(object sender, ItemCheckEventArgs e) {
        // In VirtualMode, we must update the underlying data source manually.
        // ItemCheck is called before the change is applied to the UI.
        if (e.Index >= 0 && e.Index < filteredNodes.Count) {
            filteredNodes[e.Index].IsChecked = (e.NewValue == CheckState.Checked);
            // We don't need to invalidate here as RetrieveVirtualItem will use the new value
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void FileListView_MouseDown(object sender, MouseEventArgs e) {
        // Manual hit testing for checkboxes in VirtualMode
        var info = fileListView.HitTest(e.Location);
        if (info.Item != null && info.Location == ListViewHitTestLocations.StateImage) {
            int index = info.Item.Index;
            if (index >= 0 && index < filteredNodes.Count) {
                var node = filteredNodes[index];
                node.IsChecked = !node.IsChecked;
                fileListView.Invalidate(info.Item.Bounds);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void FileListView_DoubleClick(object sender, EventArgs e) {
        if (fileListView.SelectedIndices.Count > 0) {
            var index = fileListView.SelectedIndices[0];
            var node = filteredNodes[index];
            FileDoubleClicked?.Invoke(this, node.Path);
        }
    }

    public List<string> GetSelectedFiles() => allNodes.Where(n => n.IsChecked).Select(n => n.Path).ToList();

    private void InitializeComponent() {
        this.SuspendLayout();
        this.Name = "FileTreePanel";
        this.Size = new Size(500, 600);
        this.ResumeLayout(false);
    }
}
