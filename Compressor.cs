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

    public bool CompressChild { get; set; }
    public bool Overwrite { get; set; }
    public bool CompressCompressed { get; set; }
    public SaveAs SaveAs { get; set; }
    public int MinimumFileSize { get; set; }
    public bool StripMetadata { get; set; }

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
    public event EventHandler<string> ErrorOccurred;

    private bool _operationCanRun = true;

    private readonly SemaphoreSlim _semaphore;

    public void Cancel() {
        _operationCanRun = false;
    }

    public Compressor(string compressPath, string savePath, int quality, int resize, bool compressChild, bool overwrite,
        bool compressCompressed, SaveAs saveAs, int minimumFileSize) {
        CompressPath = compressPath;
        SavePath = savePath;
        Quality = quality;
        Resize = resize;
        CompressChild = compressChild;
        Overwrite = overwrite;
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
        CompressChild = false;
        Overwrite = false;
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
        var filesToCompress = CompressChild
            ? GetAllowedFilesInChild(CompressPath, CompressCompressed, MinimumFileSize)
            : GetAllowedFiles(CompressPath, CompressCompressed, MinimumFileSize);

        filesToProcess = filesToCompress.Length;

        Directory.CreateDirectory(SavePath);

        SKEncodedImageFormat format;
        string fileExt;

        switch (SaveAs) {
            default:
            case SaveAs.JPEG:
                format = SKEncodedImageFormat.Jpeg;
                fileExt = ".jpeg";
                break;
            case SaveAs.PNG:
                format = SKEncodedImageFormat.Png;
                fileExt = ".png";
                break;
            case SaveAs.WEBP:
                format = SKEncodedImageFormat.Webp;
                fileExt = ".webp";
                break;
        }

        try {
            Task[] tasks = new Task[filesToProcess];

            for (int i = 0; i < filesToProcess; i++) {
                var file = filesToCompress[i];
                tasks[i] = Task.Run(async () => {
                    await _semaphore.WaitAsync();
                    try {
                        CompressProcess(file, format, fileExt);
                    }
                    catch (OperationCanceledException) {
                        // Handled by collective cancellation check
                    }
                    catch (Exception e) {
                        ErrorOccurred?.Invoke(this, $"Error processing {Path.GetFileName(file)}: {e.Message}");
                    }
                    finally {
                        _semaphore.Release();
                    }
                });
            }

            Task.WaitAll(tasks);

            if (!_operationCanRun) {
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

    private void CompressProcess(string file, SKEncodedImageFormat format, string fileExt) {
        if (!_operationCanRun) {
            throw new OperationCanceledException();
        }

        var imageData = File.ReadAllBytes(file);
        var compressedImageData = CompressImage(imageData, Resize, Quality, format, StripMetadata);

        var filePath = Path.GetDirectoryName(file);
        var fileName = Path.GetFileNameWithoutExtension(file);

        lock (_lockFileNameObject) {
            if (Overwrite) {
                File.Delete(file);
                var newFile = GetFileName(filePath, fileName, fileExt);
                File.WriteAllBytes(newFile, compressedImageData);
            } else {
                var pathWithoutRoot = filePath.Replace(CompressPath, "");
                Directory.CreateDirectory(SavePath + pathWithoutRoot);
                // var newFile = Path.Combine(SavePath + pathWithoutRoot, fileName + fileExt);
                // while (File.Exists(newFile))
                // newFile = Path.Combine(SavePath + pathWithoutRoot, $"{fileName}(1){fileExt}");
                var newFile = GetFileName(SavePath + pathWithoutRoot, fileName, fileExt);
                File.WriteAllBytes(newFile, compressedImageData);
            }
        }

        int currentCount;
        lock (_lockCounterObject) {
            ProcessedCount++;
            currentCount = ProcessedCount;
        }
        
        // Fire event outside the lock but with a captured value for consistency if needed, 
        // though ProcessedCount property already fires it. 
        // Wait, ProcessedCount property fires event. Let's make it consistent.
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

    private static byte[] CompressImage(byte[] imageData, int percent, int quality, SKEncodedImageFormat format, bool stripMetadata) {
        using SKBitmap sourceBitmap = SKBitmap.Decode(imageData);
        if (sourceBitmap == null) {
            return imageData;
        }

        int actualWidth = sourceBitmap.Width;
        int actualHeight = sourceBitmap.Height;

        int targetWidth = Math.Max(1, (int)Math.Floor(actualWidth / 100f * percent));
        int targetHeight = Math.Max(1, (int)Math.Floor(actualHeight / 100f * percent));

        using SKBitmap resizedBitmap =
            sourceBitmap.Resize(new SKImageInfo(targetWidth, targetHeight), SKSamplingOptions.Default);
        
        if (resizedBitmap == null) {
            return imageData; // Fallback to original if resize fails
        }

        using SKData encodedData = resizedBitmap.Encode(format, quality);
        if (encodedData == null) {
            return imageData;
        }

        if (stripMetadata || format == SKEncodedImageFormat.Webp) {
            return encodedData.ToArray();
        }

        try {
            var file = ImageFile.FromBuffer(encodedData.ToArray());
            file.Properties.Set(ExifTag.ImageDescription,
                "Image compressed by GetTheNya`s bulk image compression tool");
            using MemoryStream stream = new MemoryStream();
            file.Save(stream);
            return stream.ToArray();
        }
        catch {
            return encodedData.ToArray(); // Return compressed but without metadata if ExifLibrary fails
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
