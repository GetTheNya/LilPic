using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExifLibrary;
using SkiaSharp;

namespace BulkImageCompressor;

public class Compressor {
    public string CompressPath { get; set; }
    public string SavePath { get; set; }
    private int quality;

    public int Quality {
        get => quality;
        set {
            if (value >= 100)
                quality = 100;
            else if (value <= 10)
                quality = 10;
            else
                quality = value;
        }
    }

    private int resize;

    public int Resize {
        get => resize;
        set {
            if (value >= 100)
                resize = 100;
            else if (value <= 10)
                resize = 10;
            else
                resize = value;
        }
    }

    public bool CompressChildren { get; set; }
    public int OverwritePolicy { get; set; } // 0: Suffix, 1: Overwrite, 2: Skip
    public bool CompressCompressed { get; set; }
    public SaveAs SaveAs { get; set; }
    public int MinimumFileSize { get; set; }
    public bool StripMetadata { get; set; }
    public bool CopyNonImages { get; set; }

    public int TargetWidth { get; set; }
    public int TargetHeight { get; set; }
    public long TargetFileSizeBytes { get; set; }

    public IEnumerable<string> ExplicitFiles { get; set; }
    public bool IsDryRun { get; set; }
    public long TotalOriginalSize { get; private set; }
    public long TotalEstimatedSize { get; private set; }

    private int processedCount = 0;
    private int filesToProcess = 0;

    public int ProcessedCount {
        get => processedCount;
        private set {
            processedCount = value;
            ProcessEvent?.Invoke(this, new ProcessArgs(processedCount, filesToProcess));
        }
    }

    private readonly object _lockCounterObject = new();
    private readonly object _lockFileNameObject = new();

    public event EventHandler<ProcessArgs> ProcessEvent;
    public event EventHandler<ProcessArgs> ProcessCanceled;
    public event EventHandler<ProcessArgs> ProcessCompleted;
    public event EventHandler<(int Slot, string FileName)> WorkerActivity;
    public event EventHandler<(string FilePath, string Status, string Reason, long EstimatedSize)> FileProcessed;
    public event EventHandler<(string Message, bool IsError)> LogMessage;

    private CancellationTokenSource _cts;

    private readonly SemaphoreSlim _semaphore;

    public void Cancel() {
        _cts?.Cancel();
    }

    public Compressor(string compressPath, string savePath, int quality, int resize, bool compressChild, bool overwrite,
        bool compressCompressed, SaveAs saveAs, int minimumFileSize) {
        CompressPath = compressPath;
        SavePath = savePath;
        Quality = quality;
        Resize = resize;
        CompressChildren = compressChild;
        OverwritePolicy = overwrite ? 1 : 0;
        CompressCompressed = compressCompressed;
        SaveAs = saveAs;
        MinimumFileSize = minimumFileSize;
        StripMetadata = true;

        _semaphore = new SemaphoreSlim(Environment.ProcessorCount);
    }

    public Compressor(string compressPath) {
        CompressPath = compressPath;
        SavePath = Path.Combine(CompressPath, "Compressed");
        Quality = 80;
        Resize = 80;
        CompressChildren = false;
        OverwritePolicy = 0;
        CompressCompressed = false;
        SaveAs = SaveAs.JPEG;
        MinimumFileSize = -1;
        StripMetadata = true;

        _semaphore = new SemaphoreSlim(Environment.ProcessorCount);
    }

    public void CompressAsync() {
        Task.Run(async () => await StartCompress());
    }

    public void Compress() {
        StartCompress().GetAwaiter().GetResult();
    }

    private async Task StartCompress() {
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        var filesToCompress = ExplicitFiles?.ToArray() ?? (CompressChildren
            ? GetAllowedFilesInChild(CompressPath, CompressCompressed, MinimumFileSize)
            : GetAllowedFiles(CompressPath, CompressCompressed, MinimumFileSize));

        filesToProcess = filesToCompress.Length;
        processedCount = 0;
        TotalOriginalSize = 0;
        TotalEstimatedSize = 0;

        string mode = IsDryRun ? "Dry Run" : "Compression";
        LogMessage?.Invoke(this, ($"Starting {mode} of {filesToProcess} files...", false));

        if (!IsDryRun) Directory.CreateDirectory(SavePath);

        SKEncodedImageFormat format = SaveAs switch {
            SaveAs.PNG => SKEncodedImageFormat.Png,
            SaveAs.WEBP => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Jpeg
        };
        
        string fileExt = SaveAs switch {
            SaveAs.PNG => ".png",
            SaveAs.WEBP => ".webp",
            _ => ".jpg"
        };

        try {
            var workerSlots = new bool[Environment.ProcessorCount];
            var parallelOptions = new ParallelOptions {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(filesToCompress, parallelOptions, async (file, token) => {
                int slot = -1;
                // Assign a slot for visualization
                lock (workerSlots) {
                    slot = Array.IndexOf(workerSlots, false);
                    if (slot != -1) workerSlots[slot] = true;
                }

                try {
                    WorkerActivity?.Invoke(this, (slot, Path.GetFileName(file)));
                    CompressProcess(file, format, fileExt, token);
                }
                finally {
                    if (slot != -1) {
                        lock (workerSlots) workerSlots[slot] = false;
                        WorkerActivity?.Invoke(this, (slot, ""));
                    }
                }
            });

            if (ct.IsCancellationRequested) {
                LogMessage?.Invoke(this, ("Operation canceled by user.", true));
                ProcessCanceled?.Invoke(this, new ProcessArgs(processedCount, filesToProcess));
            } else {
                string finishMode = IsDryRun ? "Simulation" : "Processing";
                LogMessage?.Invoke(this, ($"{finishMode} completed. {processedCount} files processed.", false));
                ProcessCompleted?.Invoke(this, new ProcessArgs(processedCount, filesToProcess));
            }
        }
        catch (OperationCanceledException) {
            LogMessage?.Invoke(this, ("Operation canceled.", true));
            ProcessCanceled?.Invoke(this, new ProcessArgs(processedCount, filesToProcess));
        }
        catch (Exception ex) {
            LogMessage?.Invoke(this, ($"Error: {ex.Message}", true));
            ProcessCompleted?.Invoke(this, new ProcessArgs(processedCount, filesToProcess));
        }
    }

    private void CompressProcess(string file, SKEncodedImageFormat format, string fileExt, CancellationToken ct) {
        ct.ThrowIfCancellationRequested();

        bool isImage = AllowedExtensions.ImageExtensions.Contains(Path.GetExtension(file).ToUpper());

        if (isImage) {
            if (!CompressCompressed) {
                try {
                    var metadata = ImageFile.FromFile(file);
                    var desc = metadata.Properties.Get<ExifAscii>(ExifTag.ImageDescription);
                    if (desc != null && desc.Value.Contains("compressed", StringComparison.OrdinalIgnoreCase)) {
                        FileProcessed?.Invoke(this, (file, "⏭ Skip", "Already compressed", 0));
                        LogMessage?.Invoke(this, ($"Skipped {Path.GetFileName(file)}: Already compressed by tool.", false));
                        return; // Skip already compressed
                    }
                } catch { }
            }

            try {
                var imageData = File.ReadAllBytes(file);
                byte[] finalData = CompressImage(imageData, Resize, Quality, format, StripMetadata, TargetWidth, TargetHeight, TargetFileSizeBytes, ct);
                
                lock (_lockCounterObject) {
                    TotalOriginalSize += imageData.Length;
                    TotalEstimatedSize += finalData.Length;
                }

                if (!IsDryRun) {
                    SaveToFile(file, finalData, fileExt);
                } else {
                    lock (_lockCounterObject) ProcessedCount++;
                    FileProcessed?.Invoke(this, (file, "✅ Sim", "", (long)finalData.Length));
                }
            } catch (Exception ex) {
                FileProcessed?.Invoke(this, (file, "⚠️ Error", ex.Message, 0));
                LogMessage?.Invoke(this, ($"Error processing {Path.GetFileName(file)}: {ex.Message}", true));
            }
        } else if (CopyNonImages) {
            try {
                var data = File.ReadAllBytes(file);
                if (!IsDryRun) {
                    SaveToFile(file, data, Path.GetExtension(file));
                } else {
                    lock (_lockCounterObject) {
                        TotalOriginalSize += data.Length;
                        TotalEstimatedSize += data.Length;
                        ProcessedCount++;
                    }
                    FileProcessed?.Invoke(this, (file, "✅ Copy Sim", "", (long)data.Length));
                }
            } catch (Exception ex) {
                FileProcessed?.Invoke(this, (file, "⚠️ Error", ex.Message, 0));
            }
        }
    }

    private void SaveToFile(string originalFile, byte[] data, string ext) {
        var filePath = Path.GetDirectoryName(originalFile);
        var fileName = Path.GetFileNameWithoutExtension(originalFile);
        var relativeDir = filePath.Replace(CompressPath, "").TrimStart(Path.DirectorySeparatorChar);

        try {
            lock (_lockFileNameObject) {
                string targetFolder = string.IsNullOrEmpty(relativeDir) ? SavePath : Path.Combine(SavePath, relativeDir);
                Directory.CreateDirectory(targetFolder);

                string newFile = OverwritePolicy switch {
                    1 => originalFile, // Overwrite
                    0 => GetFileName(targetFolder, fileName, ext), // Suffix
                    _ => Path.Combine(targetFolder, fileName + ext) // Skip
                };

                if (OverwritePolicy == 2 && File.Exists(newFile)) {
                    FileProcessed?.Invoke(this, (originalFile, "⏭ Skip", "File exists", 0));
                    return;
                }

                File.WriteAllBytes(newFile, data);
                FileProcessed?.Invoke(this, (originalFile, "✅ Done", "", (long)data.Length));
            }
        } catch (Exception ex) {
            FileProcessed?.Invoke(this, (originalFile, "⚠️ Error", ex.Message, 0));
        }

        lock (_lockCounterObject) {
            ProcessedCount++;
        }
    }

    private string GetFileName(string filePath, string fileName, string fileExt) {
        var newFileName = Path.Combine(filePath, fileName + fileExt);

        if (!File.Exists(newFileName)) return newFileName;

        int counter = 1;
        while (File.Exists(newFileName)) {
            newFileName = Path.Combine(filePath, fileName + $"({counter++})" + fileExt);
        }

        return newFileName;
    }

    private static string[] GetAllowedFilesInChild(string path, bool compressCompressed, int minimumFileSize = -1) {
        var childFolders = Directory.GetDirectories(path);
        List<string> files = new();
        files.AddRange(GetAllowedFiles(path, compressCompressed, minimumFileSize));
        foreach (var folder in childFolders) {
            files.AddRange(GetAllowedFilesInChild(folder, compressCompressed, minimumFileSize));
        }

        return files.ToArray();
    }

    public static byte[] CompressImage(byte[] imageData, int percent, int quality, SKEncodedImageFormat format, bool stripMetadata, int targetWidth = 0, int targetHeight = 0, long targetFileSizeBytes = 0, CancellationToken ct = default) {
        ct.ThrowIfCancellationRequested();
        
        using SKBitmap sourceBitmap = SKBitmap.Decode(imageData);
        if (sourceBitmap == null) return imageData;

        int finalWidth, finalHeight;
        if (targetWidth > 0 && targetHeight > 0) {
            float ratio = Math.Min((float)targetWidth / sourceBitmap.Width, (float)targetHeight / sourceBitmap.Height);
            finalWidth = Math.Max(1, (int)(sourceBitmap.Width * ratio));
            finalHeight = Math.Max(1, (int)(sourceBitmap.Height * ratio));
        } else {
            finalWidth = Math.Max(1, (int)Math.Floor(sourceBitmap.Width / 100f * percent));
            finalHeight = Math.Max(1, (int)Math.Floor(sourceBitmap.Height / 100f * percent));
        }

        using SKBitmap resizedBitmap = sourceBitmap.Resize(new SKImageInfo(finalWidth, finalHeight), SKSamplingOptions.Default);
        if (resizedBitmap == null) return imageData;

        int finalQuality = quality;
        if (targetFileSizeBytes > 0) {
            finalQuality = ImageSizeEstimator.EstimateQuality(imageData, targetFileSizeBytes, percent, format, stripMetadata);
        }

        ct.ThrowIfCancellationRequested();
        using SKData encodedData = resizedBitmap.Encode(format, finalQuality);
        if (encodedData == null) return imageData;

        if (stripMetadata || format == SKEncodedImageFormat.Webp) {
            return encodedData.ToArray();
        }

        try {
            var file = ImageFile.FromBuffer(encodedData.ToArray());
            file.Properties.Set(ExifTag.ImageDescription, "Compressed");
            using MemoryStream stream = new MemoryStream();
            file.Save(stream);
            return stream.ToArray();
        }
        catch {
            return encodedData.ToArray();
        }
    }

    private static string[] GetAllowedFiles(string path, bool compressCompressed, int minimumFileSize = -1) {
        return Directory.GetFiles(path, "*.*").Where(file => {
            if (!compressCompressed) {
                try {
                    var fileMetaData = ImageFile.FromFile(file);
                    var imageDescription = fileMetaData.Properties.Get<ExifAscii>(ExifTag.ImageDescription);
                    if (imageDescription != null) {
                        if (imageDescription.Value.Contains("compressed", StringComparison.OrdinalIgnoreCase)) return false;
                    }
                }
                catch (Exception) {
                    //ignored
                }
            }

            var fileInfo = new FileInfo(file);
            var isAllowedExtension = AllowedExtensions.ImageExtensions.Contains(Path.GetExtension(file).ToUpper());

            if (minimumFileSize == -1) return isAllowedExtension;
            return isAllowedExtension && fileInfo.Length > minimumFileSize;
        }).ToArray();
    }
}
