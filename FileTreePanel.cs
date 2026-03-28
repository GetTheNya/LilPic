using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LilPic;

public partial class FileTreePanel : UserControl {
    private ListView fileListView;
    private TreeView folderTreeView;
    private SplitContainer splitContainer;
    private TextBox searchBox;
    private CheckBox regexCBox;
    private FlowLayoutPanel filterPanel;
    private FlowLayoutPanel toolsPanel;
    private FlowLayoutPanel extensionsPanel;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel countLabel;
    private ToolStripStatusLabel resStatusLabel;
    private CheckBox folderViewToggle;

    private List<FileNode> allNodes = new();
    private List<FileNode> filteredNodes = new();
    private string activeFilter = "All";
    private string currentRootPath = "";
    private string selectedFolderPath = "";
    
    // Sort state
    private int sortColumn = -1;
    private bool sortAscending = true;

    private bool skipSize;
    private long minSizeKB;
    private bool skipRes;
    private int minW;
    private int minH;

    public bool IsCalculatingResolutions { get; private set; }

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
        toolsPanel = new FlowLayoutPanel { 
            Dock = DockStyle.Top, 
            AutoSize = true,
            Padding = new Padding(5)
        };

        filterPanel = new FlowLayoutPanel { 
            Dock = DockStyle.Top, 
            AutoSize = true,
            Padding = new Padding(5)
        };

        extensionsPanel = new FlowLayoutPanel {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(5)
        };

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
        fileListView.Columns.Add("Comp. Size", 90);
        fileListView.Columns.Add("Res", 100);
        fileListView.Columns.Add("Status", 80);

        fileListView.RetrieveVirtualItem += FileListView_RetrieveVirtualItem;
        fileListView.ItemCheck += FileListView_ItemCheck;
        fileListView.DoubleClick += FileListView_DoubleClick;
        fileListView.MouseDown += FileListView_MouseDown;
        fileListView.ColumnClick += FileListView_ColumnClick;

        splitContainer.Panel1.Controls.Add(folderTreeView);
        splitContainer.Panel2.Controls.Add(fileListView);

        // Status Strip
        statusStrip = new StatusStrip();
        countLabel = new ToolStripStatusLabel("0 files found");
        resStatusLabel = new ToolStripStatusLabel("") { 
            Alignment = ToolStripItemAlignment.Right, 
            ForeColor = Color.DarkBlue,
            Spring = true,
            TextAlign = ContentAlignment.MiddleRight
        };
        statusStrip.Items.Add(countLabel);
        statusStrip.Items.Add(resStatusLabel);

        this.Controls.Add(splitContainer);
        this.Controls.Add(extensionsPanel);
        this.Controls.Add(filterPanel);
        this.Controls.Add(toolsPanel);
        this.Controls.Add(topPanel);
        this.Controls.Add(statusStrip);

        // Initial chips (placeholder, will be updated on load)
        UpdateFilterChips();
    }

    private void UpdateFilterChips() {
        if (fileListView.InvokeRequired) {
            this.BeginInvoke(new Action(UpdateFilterChips));
            return;
        }

        toolsPanel.Controls.Clear();
        filterPanel.Controls.Clear();
        extensionsPanel.Controls.Clear();
        
        // --- Row 1: Selection Tools ---
        var selectAllBtn = new Button { Text = "☑ Check All", AutoSize = true, Margin = new Padding(2) };
        selectAllBtn.Click += (s, e) => { foreach(var n in filteredNodes) n.IsChecked = true; RefreshList(); UpdateStatusLabel(); SelectionChanged?.Invoke(this, EventArgs.Empty); };
        
        var unselectAllBtn = new Button { Text = "☐ Uncheck All", AutoSize = true, Margin = new Padding(2) };
        unselectAllBtn.Click += (s, e) => { foreach(var n in filteredNodes) { n.IsChecked = false; n.IsManuallyUnchecked = true; } RefreshList(); UpdateStatusLabel(); SelectionChanged?.Invoke(this, EventArgs.Empty); };
        
        toolsPanel.Controls.Add(selectAllBtn);
        toolsPanel.Controls.Add(unselectAllBtn);

        // --- Row 2: General Filters ---
        AddFilterChip("All", filterPanel);
        AddFilterChip("Checked", filterPanel);
        AddFilterChip("Large (> 5MB)", filterPanel);
        AddFilterChip("Medium (1-5MB)", filterPanel);
        AddFilterChip("Small (< 1MB)", filterPanel);

        var clearBtn = new Button {
            Text = "✖ Clear All",
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Red,
            AutoSize = true,
            Margin = new Padding(2)
        };
        clearBtn.Click += (s, e) => {
            searchBox.Text = "";
            activeFilter = "All";
            ResetChipColors();
            ApplyFilter();
        };
        filterPanel.Controls.Add(clearBtn);

        // --- Row 3: Extension Filters ---
        var extensions = allNodes
            .Select(n => Path.GetExtension(n.Path).ToUpper().TrimStart('.'))
            .Select(e => e == "JPEG" ? "JPG" : e)
            .Distinct()
            .OrderBy(e => e);

        foreach (var ext in extensions) {
            if (!string.IsNullOrEmpty(ext)) AddFilterChip(ext, extensionsPanel);
        }
    }

    private void AddFilterChip(string text, FlowLayoutPanel panel) {
        var btn = new Button { 
            Text = text, 
            FlatStyle = FlatStyle.Flat,
            BackColor = (text == activeFilter) ? Color.LightBlue : Color.White,
            AutoSize = true,
            Margin = new Padding(2),
            Tag = "Chip"
        };
        btn.Click += (s, e) => {
            activeFilter = btn.Text;
            ResetChipColors();
            ApplyFilter();
        };
        panel.Controls.Add(btn);
    }

    private void ResetChipColors() {
        var panels = new[] { filterPanel, extensionsPanel };
        foreach (var p in panels) {
            foreach (Control c in p.Controls) {
                if (c is Button b && b.Tag as string == "Chip") {
                    b.BackColor = (b.Text == activeFilter) ? Color.LightBlue : Color.White;
                }
            }
        }
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
        UpdateFilterChips();
        ApplyFilter();

        // Background resolution loading
        Task.Run(() => {
            IsCalculatingResolutions = true;
            int count = 0;
            int total = allNodes.Count;
            
            foreach (var node in allNodes) {
                try {
                    using (var stream = File.OpenRead(node.Path))
                    using (var codec = SkiaSharp.SKCodec.Create(stream)) {
                        if (codec != null) {
                            node.Width = codec.Info.Width;
                            node.Height = codec.Info.Height;
                            node.Resolution = $"{node.Width}x{node.Height}";
                        }
                    }
                } catch { }
                count++;
                if (count % 10 == 0 || count == total) {
                    string status = count == total ? "" : $"Loading resolutions... {count}/{total}";
                    this.Invoke(new Action(() => resStatusLabel.Text = status));
                    if (count % 50 == 0) {
                        if (skipRes) this.Invoke(new Action(() => ApplySkipFilters()));
                        RefreshList();
                    }
                }
            }
            if (skipRes) this.Invoke(new Action(() => ApplySkipFilters()));
            IsCalculatingResolutions = false;
            RefreshList();
        });
    }

    public void SetSkipFilters(bool skipSize, long minSizeKB, bool skipRes, int minW, int minH) {
        this.skipSize = skipSize;
        this.minSizeKB = minSizeKB;
        this.skipRes = skipRes;
        this.minW = minW;
        this.minH = minH;
        ApplySkipFilters();
    }

    private void ApplySkipFilters() {
        bool changed = false;
        foreach (var node in allNodes) {
            bool shouldSkip = false;
            if (skipSize && node.Size < minSizeKB * 1024) shouldSkip = true;
            if (!shouldSkip && skipRes && node.Width > 0 && node.Height > 0 && (node.Width < minW || node.Height < minH)) shouldSkip = true;

            bool nextChecked;
            if (shouldSkip) {
                nextChecked = false;
            } else {
                nextChecked = !node.IsManuallyUnchecked;
            }

            if (node.IsChecked != nextChecked) {
                node.IsChecked = nextChecked;
                changed = true;
            }
        }
        if (changed) {
            RefreshList();
            UpdateStatusLabel();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void FileListView_ColumnClick(object sender, ColumnClickEventArgs e) {
        if (e.Column == sortColumn) {
            sortAscending = !sortAscending;
        } else {
            sortColumn = e.Column;
            sortAscending = true;
        }
        ApplySort();
    }

    private void ApplySort() {
        if (sortColumn == -1) return;

        filteredNodes.Sort((x, y) => {
            int result = sortColumn switch {
                0 => string.Compare(x.Name, y.Name),
                1 => x.Size.CompareTo(y.Size),
                2 => (x.EstimatedSize ?? 0).CompareTo(y.EstimatedSize ?? 0),
                3 => (x.Width * x.Height).CompareTo(y.Width * y.Height),
                4 => string.Compare(x.Status, y.Status),
                _ => 0
            };
            return sortAscending ? result : -result;
        });
        fileListView.Invalidate();
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
            case "Checked":
                query = query.Where(n => n.IsChecked);
                break;
            case "Large (> 5MB)":
                query = query.Where(n => n.Size > 5 * 1024 * 1024);
                break;
            case "Medium (1-5MB)":
                query = query.Where(n => n.Size >= 1024 * 1024 && n.Size <= 5 * 1024 * 1024);
                break;
            case "Small (< 1MB)":
                query = query.Where(n => n.Size < 1024 * 1024);
                break;
            case "All":
                break;
            default:
                string filter = activeFilter;
                if (filter == "JPG") {
                    query = query.Where(n => Path.GetExtension(n.Path).Equals(".JPG", StringComparison.OrdinalIgnoreCase) || 
                                             Path.GetExtension(n.Path).Equals(".JPEG", StringComparison.OrdinalIgnoreCase));
                } else {
                    query = query.Where(n => Path.GetExtension(n.Path).Equals("." + filter, StringComparison.OrdinalIgnoreCase));
                }
                break;
        }

        filteredNodes = query.ToList();
        ApplySort();
        fileListView.VirtualListSize = filteredNodes.Count;
        UpdateStatusLabel();
        fileListView.Invalidate();
    }

    private void UpdateStatusLabel() {
        int selected = allNodes.Count(n => n.IsChecked);
        countLabel.Text = $"{filteredNodes.Count} files shown ({selected} selected, {allNodes.Count} total images)";
    }

    private void FileListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
        if (e.ItemIndex >= 0 && e.ItemIndex < filteredNodes.Count) {
            var node = filteredNodes[e.ItemIndex];
            var item = new ListViewItem(node.Name) { Checked = node.IsChecked };
            item.SubItems.Add(Utils.FormatSize(node.Size));
            item.SubItems.Add(node.EstimatedSize.HasValue ? Utils.FormatSize(node.EstimatedSize.Value) : "—");
            item.SubItems.Add(node.Resolution);
            item.SubItems.Add(node.Status);

            if (node.Size > 10 * 1024 * 1024) item.ForeColor = Color.Red;
            else if (node.Size > 2 * 1024 * 1024) item.ForeColor = Color.Orange;

            e.Item = item;
        }
    }

    private void FileListView_ItemCheck(object sender, ItemCheckEventArgs e) {
        if (e.Index >= 0 && e.Index < filteredNodes.Count) {
            var node = filteredNodes[e.Index];
            bool newValue = (e.NewValue == CheckState.Checked);
            node.IsChecked = newValue;
            node.IsManuallyUnchecked = !newValue;
            UpdateStatusLabel();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void FileListView_MouseDown(object sender, MouseEventArgs e) {
        var info = fileListView.HitTest(e.Location);
        if (info.Item != null && info.Location == ListViewHitTestLocations.StateImage) {
            int index = info.Item.Index;
            if (index >= 0 && index < filteredNodes.Count) {
                var node = filteredNodes[index];
                node.IsChecked = !node.IsChecked;
                node.IsManuallyUnchecked = !node.IsChecked;
                UpdateStatusLabel();
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

    public void RefreshList() {
        if (fileListView.InvokeRequired) {
            fileListView.BeginInvoke(new Action(RefreshList));
            return;
        }
        fileListView.Invalidate();
    }

    public void UpdateNodeStatus(string filePath, string status, string reason, long size = 0) {
        var node = allNodes.FirstOrDefault(n => n.Path == filePath);
        if (node != null) {
            node.Status = status;
            node.Reason = reason;
            if (size > 0) node.EstimatedSize = size;
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
