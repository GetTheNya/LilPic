using System;

namespace BulkImageCompressor;

public static class Utils {
    public static string FormatSize(long bytes) {
        string[] units = { "B", "KB", "MB", "GB" };
        int unitIndex = 0;
        double size = bytes;
        while (size >= 1024 && unitIndex < units.Length - 1) {
            size /= 1024;
            unitIndex++;
        }
        return $"{size:F2} {units[unitIndex]}";
    }
}
