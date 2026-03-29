using System;
using System.IO;
using System.Text.Json;

namespace LilPic.Models;

public class AppSettings {
    public string LastInputFolder { get; set; }
    public string LastOutputFolder { get; set; }
    public int Quality { get; set; } = 80;
    public int ResizePercent { get; set; } = 80;
    public int ResizeMode { get; set; } = 0; // 0: %, 1: px, 2: target size
    public int TargetWidth { get; set; } = 1920;
    public int TargetHeight { get; set; } = 1080;
    public long TargetFileSizeKB { get; set; } = 500;
    public bool StripMetadata { get; set; } = true;
    public bool CopyNonImages { get; set; } = false;
    public bool CopySkippedImages { get; set; } = false;
    public int OverwritePolicy { get; set; } = 0; // 0: Suffix, 1: Overwrite, 2: Skip
    public bool CompressAlreadyCompressed { get; set; } = false;
    public int SaveAsFormat { get; set; } = 0; // JPEG, PNG, WEBP

    // Skip Logic
    public bool SkipIfSmallerThanSize { get; set; } = false;
    public long MinSizeToProcessKB { get; set; } = 100;
    public bool SkipIfSmallerThanRes { get; set; } = false;
    public int MinWidthToProcess { get; set; } = 500;
    public int MinHeightToProcess { get; set; } = 500;

    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LilPic", "settings.json");

    public void Save() {
        try {
            string dir = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        } catch { }
    }

    public static AppSettings Load() {
        try {
            if (File.Exists(SettingsPath)) {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json);
            }
        } catch { }
        return new AppSettings();
    }
}
