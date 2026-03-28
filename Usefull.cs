using System;

namespace LilPic;

public enum SaveAs {
    JPEG,
    PNG,
    WEBP,
}


public class ProcessArgs : EventArgs {
    public int ProcessedFiles { get; private set; }
    public int AllFiles { get; private set; }

    public ProcessArgs(int processedFiles, int allFiles) {
        ProcessedFiles = processedFiles;
        AllFiles = allFiles;
    }
}