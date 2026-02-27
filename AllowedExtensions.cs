using System.Collections.Generic;

namespace BulkImageCompressor;

public static class AllowedExtensions {
    public static readonly HashSet<string> ImageExtensions = new() {
        ".JPG", ".JPEG", ".PNG", ".WEBP", ".BMP", ".GIF", ".TIFF"
    };
    public static bool IsImage(string path) {
        string ext = System.IO.Path.GetExtension(path).ToUpperInvariant();
        return ImageExtensions.Contains(ext);
    }
}
