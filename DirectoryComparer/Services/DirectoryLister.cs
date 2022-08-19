using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DirectoryComparer.Services
{
    public class DirectoryLister
    {
        public static List<string> GetAllFiles(string rootFolder, bool isRecursive)
        {
            if (!Directory.Exists(rootFolder)) throw new Exception("Root folder does not exist");

            var option = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(rootFolder, "*.*", option);

            return files.ToList();
        }

        public static List<string> GetAllFiles(string rootFolder)
        {
            if (!Directory.Exists(rootFolder)) throw new Exception("Root folder does not exist");

            var filesAndFolders = Directory.GetFiles(rootFolder, "*.*", SearchOption.TopDirectoryOnly);
            filesAndFolders = filesAndFolders.Concat(Directory.GetDirectories(rootFolder)).ToArray();

            return filesAndFolders.ToList();
        }
    }
}