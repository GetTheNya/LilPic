using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ShellProgressBar;

namespace LilPic;

public class ConsoleCompressor {
    private readonly List<string> _args;

    private Compressor _compressor;

    private static readonly string AppName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName);

    private readonly string HELP = $"""
-----Help for compressor in console mode-----
Usage: {AppName} <path> [options]

Arguments:
  <path>                                    Path to the input folder containing images to be compressed

Options:
  -help, --h                                Show help guide
  -output, --o <path>                       Set the output folder path (default: path from first argument + /Compressed)
  -quality, --q <number>                    Set the compression quality (1-100) where 100 is the best (default: 80)
  -resize, --rs <number>                    Set the resize percentage (1-100) where 100 is the original size (default: 80)
  -compresschild, --cc                      Enable compressing images in child folders (default: not set)
  -overwrite, --ow                          Enable overwriting images instead of saving to another folder (default: not set)
  -compressbiggerthan, --cbt <size in kb>   Compress images larger than the specified size (default: not set)
  -stripmetadata, --sm                      Strip EXIF metadata to reduce file size (default: set)
  -jpeg                                     Save images in JPEG format (default)
  -png                                      Save images in PNG format

Note:
  - Only one saving format can be specified at a time (either -jpeg or -png).
  - The options in square brackets are optional.

Examples:
  {AppName} /path/to/images -output /path/to/output -quality 90 -resize 50 -compresschild -jpeg
  {AppName} /path/to/images --cbt 1024 -overwrite -png
""";

    public ConsoleCompressor(string[] args) {
        _args = new(args);
        StartConsole();
        ParseArgs();
        PrintSettings();
        if (IsStartCompress()) {
            StartCompress();
        }

        CloseConsole();
    }

    private void StartConsole() {
        NativeMethods.AllocConsole();
        Console.WriteLine("Program arguments found! Running in console mode...");
        Console.WriteLine("---------\n");
    }

    private void CloseConsole() {
        Console.WriteLine("\n---------");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        NativeMethods.FreeConsole();
    }

    private void ParseArgs() {
        if (_args.Contains("-help") || _args.Contains("--h")) {
            Console.WriteLine(HELP);
            CloseConsole();
            return;
        }

        if (_args.Contains("-jpeg") && _args.Contains("-png")) {
            Console.WriteLine("Set one output file extention");
            CloseConsole();
            return;
        }

        if (!Directory.Exists(_args[0])) {
            Console.WriteLine("First argument must be a path to the folder!");
            Console.WriteLine("Start this app with \"-help\" for help");
            CloseConsole();
            return;
        }

        _compressor = new Compressor(_args[0]);

        for (var i = 0; i < _args.Count; i++) {
            var arg = _args[i];

            if (arg is "-output" or "--o") {
                if (_args.Count > i + 1 && !_args[i + 1].StartsWith("-"))
                    _compressor.SavePath = _args[i + 1];
                else {
                    Console.WriteLine($"Set correct path after {arg} argument");
                    CloseConsole();
                    return;
                }
            } else if (arg is "-quality" or "--q") {
                if (_args.Count > i + 1 && int.TryParse(_args[i + 1], out var quality)) {
                    _compressor.Quality = quality;
                } else {
                    Console.WriteLine($"Set correct quality after {arg} argument");
                    CloseConsole();
                    return;
                }
            } else if (arg is "-resize" or "--rs") {
                if (_args.Count > i + 1 && int.TryParse(_args[i + 1], out var size)) {
                    _compressor.Resize = size;
                } else {
                    Console.WriteLine($"Set correct resize after {arg} argument");
                    CloseConsole();
                    return;
                }
            } else if (arg is "-compresschild" or "--cc") {
                _compressor.CompressChildren = true;
            } else if (arg is "-overwrite" or "--ow") {
                _compressor.OverwritePolicy = 1;
            } else if (arg is "-compressbiggerthan" or "--cbt") {
                if (_args.Count > i + 1 && int.TryParse(_args[i + 1], out var size)) {
                    _compressor.MinimumFileSize = size * 1024;
                } else {
                    Console.WriteLine($"Set correct size after {arg} argument");
                    CloseConsole();
                    return;
                }
            } else if (arg is "-jpeg") {
                _compressor.SaveAs = SaveAs.JPEG;
            } else if (arg is "-png") {
                _compressor.SaveAs = SaveAs.PNG;
            } else if (arg is "-stripmetadata" or "--sm") {
                _compressor.StripMetadata = true;
            }
        }
    }

    private void PrintSettings() {
        var settings = $"-----Settings for compression-----\n" + $"Input Folder: {_compressor.CompressPath}\n";
        settings += _compressor.OverwritePolicy == 1
            ? "Overwrite Original Images: True\n"
            : $"Output Folder: {_compressor.SavePath}\n";
        settings += $"Compression Quality: {_compressor.Quality.ToString()}\n" +
                    $"Resize Percentage: {_compressor.Resize.ToString()}\n" +
                    $"Compress Images in Child Folders: {_compressor.CompressChildren.ToString()}\n" +
                    "Compress Images Larger Than: ";
        settings += _compressor.MinimumFileSize == -1 ? "Disabled\n" : _compressor.MinimumFileSize / 1024 + " KB\n";
        settings += "Strip Metadata: " + _compressor.StripMetadata + "\n";
        settings += "Saving Format: " + _compressor.SaveAs;

        Console.WriteLine(settings);
    }

    private bool IsStartCompress() {
        Console.WriteLine("\nPress any key to break in 5 seconds:");

        if (WaitForKeyPressAsync(TimeSpan.FromSeconds(5))) {
            Console.WriteLine("Input detected. Breaking...");
            return false;
        }

        Console.WriteLine("Compressing now!");

        return true;
    }

    static bool WaitForKeyPressAsync(TimeSpan timeout) {
        DateTime startTime = DateTime.Now;
        while (DateTime.Now - startTime < timeout) {
            if (Console.KeyAvailable) {
                Console.ReadKey();
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                return true;
            }

            Thread.Sleep(50);
        }

        return false;
    }

    private void StartCompress() {
        Console.WriteLine();
        
        var options = new ProgressBarOptions { ProgressCharacter = '─', ProgressBarOnBottom = true };
        using var pbar = new ShellProgressBar.ProgressBar(10000, "Compressing...", options);
        IProgress<float> progress = pbar.AsProgress<float>();

        _compressor.ProcessEvent += (o, a) => {
            int percentage = (a.ProcessedFiles * 100) / a.AllFiles;
            progress.Report(percentage);
        };

        _compressor.ProcessCompleted += (o, a) => {
            progress.Report(1f);
        };
        
        _compressor.Compress();
    }
}