using System;

namespace BulkImageCompressor;

public class FileNode {
    public string Path { get; set; }
    public string Name => System.IO.Path.GetFileName(Path);
    public long Size { get; set; }
    public string Resolution { get; set; } = "—";
    public bool IsImage { get; set; }
    public bool IsChecked { get; set; } = true;
    public bool AlreadyCompressed { get; set; }
    
    // Status during/after run
    public string Status { get; set; } = ""; // ✅ Done, ⚠️ Error, ⏭ Skip, etc.
    public string Reason { get; set; } = "";
    
    // Estimates
    public long? EstimatedSizeBytes { get; set; }

    public FileNode(string path) {
        Path = path;
        try {
            var info = new System.IO.FileInfo(path);
            Size = info.Length;
            string ext = System.IO.Path.GetExtension(path).ToUpper();
            IsImage = AllowedExtensions.ImageExtensions.Contains(ext);
        } catch { }
    }
}
