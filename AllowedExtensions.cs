using System.Collections.Generic;

namespace BulkImageCompressor;

public static class AllowedExtensions {
    public static readonly HashSet<string> ImageExtensions = new() {
        ".JPG", ".JPEG", ".PNG", ".WEBP", ".BMP", ".GIF", ".TIFF"
    };
}
