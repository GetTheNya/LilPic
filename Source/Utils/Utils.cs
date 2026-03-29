using System;
using System.Drawing;
using System.IO;

namespace LilPic.Utils;

public static class CommonUtils {
    private static Icon _appIcon;
    public static Icon AppIcon {
        get {
            if (_appIcon == null) {
                try {
                    var asm = typeof(CommonUtils).Assembly;
                    using var stream = asm.GetManifestResourceStream("LilPic.assets.icon.ico");
                    if (stream != null) {
                        _appIcon = new Icon(stream);
                    } else {
                        // Fallback to loose file if resource not found
                        if (File.Exists("assets/icon.ico")) _appIcon = new Icon("assets/icon.ico");
                    }
                } catch {
                    // Ignore icon loading errors
                }
            }
            return _appIcon;
        }
    }

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
