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
        Task.Run(StartCompress);
    }

    public void Compress() {
        StartCompress();
    }

    private void StartCompress() {
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        var filesToCompress = ExplicitFiles?.ToArray() ?? (CompressChildren
            ? GetAllowedFilesInChild(CompressPath, CompressCompressed, MinimumFileSize)
            : GetAllowedFiles(CompressPath, CompressCompressed, MinimumFileSize));

        filesToProcess = filesToCompress.Length;
        ProcessedCount = 0;

        Directory.CreateDirectory(SavePath);

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
            var tasks = filesToCompress.Select(async (file) => {
                int slot = -1;
                await _semaphore.WaitAsync(ct);
                try {
                    // Assign a slot for visualization
                    lock (workerSlots) {
                        slot = Array.IndexOf(workerSlots, false);
                        if (slot != -1) workerSlots[slot] = true;
                    }

                    WorkerActivity?.Invoke(this, (slot, Path.GetFileName(file)));
                    CompressProcess(file, format, fileExt, ct);
                }
                finally {
                    if (slot != -1) {
                        lock (workerSlots) workerSlots[slot] = false;
                        WorkerActivity?.Invoke(this, (slot, ""));
                    }
                    _semaphore.Release();
                }
            }).ToArray();

            Task.WaitAll(tasks);

            if (ct.IsCancellationRequested) {
                ProcessCanceled?.Invoke(this, new ProcessArgs(processedCount, filesToProcess));
            } else {
                ProcessCompleted?.Invoke(this, new ProcessArgs(processedCount, filesToProcess));
            }
        }
        catch (AggregateException ex) {
            if (ex.InnerExceptions.Any(e => e is OperationCanceledException))
                ProcessCanceled?.Invoke(this, new ProcessArgs(processedCount, filesToProcess));
        }
    }

    private void CompressProcess(string file, SKEncodedImageFormat format, string fileExt, CancellationToken ct) {
        ct.ThrowIfCancellationRequested();

        byte[] finalData;
        bool isImage = AllowedExtensions.ImageExtensions.Contains(Path.GetExtension(file).ToUpper());

        if (isImage) {
            var imageData = File.ReadAllBytes(file);
            finalData = CompressImage(imageData, Resize, Quality, format, StripMetadata, TargetWidth, TargetHeight, TargetFileSizeBytes, ct);
        } else if (CopyNonImages) {
            finalData = File.ReadAllBytes(file);
        } else {
            return; // Skip
        }

        var filePath = Path.GetDirectoryName(file);
        var fileName = Path.GetFileNameWithoutExtension(file);
        var relativeDir = filePath.Replace(CompressPath, "").TrimStart(Path.DirectorySeparatorChar);

        lock (_lockFileNameObject) {
            string targetFolder = string.IsNullOrEmpty(relativeDir) ? SavePath : Path.Combine(SavePath, relativeDir);
            Directory.CreateDirectory(targetFolder);

            string newFile = OverwritePolicy switch {
                1 => file, // Overwrite
                0 => GetFileName(targetFolder, fileName, fileExt), // Suffix
                _ => Path.Combine(targetFolder, fileName + fileExt) // Skip (handled below)
            };

            if (OverwritePolicy == 2 && File.Exists(newFile)) return;

            File.WriteAllBytes(newFile, finalData);
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
                        if (imageDescription.Value.Contains("compressed")) return false;
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
