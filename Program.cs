using System;
using System.Windows.Forms;

namespace LilPic;

static class Program {
    [STAThread]
    static void Main(string[] args) {
        if (args.Length == 0) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        } else {
            new ConsoleCompressor(args);
        }
    }
}