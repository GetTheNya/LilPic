using System;
using System.Drawing;
using System.IO;

namespace LilPic;

public static class Utils {
    private static Icon _appIcon;
    public static Icon AppIcon {
        get {
            if (_appIcon == null && File.Exists("icon.ico")) {
                try {
                    _appIcon = new Icon("icon.ico");
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
