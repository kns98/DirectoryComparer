using System;
using System.Diagnostics;

namespace DirectoryComparer.Services
{
    public static class FileOrFolderActions
    {
        public static void OpenFile(string fileName)
        {
            var windir = Environment.GetEnvironmentVariable("WINDIR");
            var process = new Process();
            process.StartInfo.FileName = windir + @"\\notepad.exe";
            process.StartInfo.Arguments = fileName;
            process.Start();
        }

        public static void OpenFolder(string folderName)
        {
            var windir = Environment.GetEnvironmentVariable("WINDIR");
            var process = new Process();
            process.StartInfo.FileName = windir + @"\\explorer.exe";
            process.StartInfo.Arguments = folderName;
            process.Start();
        }
    }
}