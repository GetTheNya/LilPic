using System;

namespace BulkImageCompressor;

public enum SaveAs {
    JPEG,
    PNG,
    WEBP,
}

public class AllowedExtensions {
    public static readonly string[] ImageExtensions = { ".JPEG", ".PNG", ".JPE", ".JPG", ".WEBP" };
}

public class ProcessArgs : EventArgs {
    public int ProcessedFiles { get; private set; }
    public int AllFiles { get; private set; }

    public ProcessArgs(int processedFiles, int allFiles) {
        ProcessedFiles = processedFiles;
        AllFiles = allFiles;
    }
}