using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectoryComparer.Interfaces;
using DirectoryComparer.Objects;
using DirectoryComparer.Services;

namespace DirectoryComparer.Comparers
{
    public class RecursiveComparer : ITwoPassComparer
    {
        private readonly frmMain _reference;

        public RecursiveComparer(frmMain reference)
        {
            _reference = reference;
        }

        public List<CompareResult> LeftCompare(string leftFolder, string rightFolder)
        {
            var _leftResults = new List<CompareResult>();

            var leftFiles = DirectoryLister.GetAllFiles(leftFolder);
            var rightFiles = DirectoryLister.GetAllFiles(rightFolder);

            _reference.ReportProgress(15);

            foreach (var fileOrFolder in leftFiles)
                if (fileOrFolder.IsFile())
                    _leftResults.Add(ProcessFile(fileOrFolder, rightFiles, CompareDirection.Left));
                else if (DirectoryComparerBaseInfo.Recursive)
                    _leftResults.AddRange(ProcessFolder(fileOrFolder, rightFiles, CompareDirection.Left));

            _reference.ReportProgress(50);

            return _leftResults;
        }

        public List<CompareResult> RightCompare(string rightFolder, string leftFolder)
        {
            var _rightResults = new List<CompareResult>();

            var leftFiles = DirectoryLister.GetAllFiles(leftFolder);
            var rightFiles = DirectoryLister.GetAllFiles(rightFolder);

            _reference.ReportProgress(70);

            foreach (var fileOrFolder in rightFiles)
                if (fileOrFolder.IsFile())
                {
                    var result = ProcessFile(fileOrFolder, leftFiles, CompareDirection.Right);
                    if (IsNotPresent(result))
                        _rightResults.Add(result);
                }
                else if (DirectoryComparerBaseInfo.Recursive)
                {
                    var results = ProcessFolder(fileOrFolder, leftFiles, CompareDirection.Right);
                    _rightResults.AddRange(GetRightOnly(results));
                }

            _reference.ReportProgress(90);

            return _rightResults;
        }

        private List<CompareResult> ProcessFolder(string fileOrFolder, List<string> otherFiles,
            CompareDirection direction)
        {
            var results = new List<CompareResult>();

            var dirName = '\\' + fileOrFolder.GetCurrentDir();
            var correspondingDir = otherFiles.SingleOrDefault(r => r.IsDirectory() && r.EndsWith(dirName));
            if (correspondingDir != null)
            {
                var leftFiles = DirectoryLister.GetAllFiles(fileOrFolder);
                var cRightFiles = DirectoryLister.GetAllFiles(correspondingDir);

                if (leftFiles.Count == 0 && cRightFiles.Count == 0)
                    results.Add(ProcessEmptyDirectory(fileOrFolder, correspondingDir, direction));
                else
                    foreach (var fileorFolder in leftFiles)
                        results.AddRange(ProcessByType(fileorFolder, cRightFiles, direction));
            }
            else
            {
                var files = DirectoryLister.GetAllFiles(fileOrFolder);

                if (files.Count > 0)
                    foreach (var file in files)
                        results.AddRange(ProcessByType(file, null, direction));
                else
                    results.Add(ProcessEmptyDirectory(fileOrFolder, string.Empty, direction));
            }

            return results;
        }

        private CompareResult ProcessEmptyDirectory(string currentFolder, string correspondingFolder,
            CompareDirection direction)
        {
            var result = new CompareResult();
            result.FileName = string.Empty;
            result.FileExtension = string.Empty;

            if (direction == CompareDirection.Left)
            {
                result.LeftFilePath = currentFolder;
                result.RightFilePath = correspondingFolder;
                result.LeftCreatedDate = currentFolder != string.Empty
                    ? Directory.GetCreationTime(currentFolder)
                    : DateTime.MinValue;
                result.LeftModifiedDate = currentFolder != string.Empty
                    ? Directory.GetLastWriteTime(currentFolder)
                    : DateTime.MinValue;
                result.RightCreatedDate = correspondingFolder != string.Empty
                    ? Directory.GetCreationTime(correspondingFolder)
                    : DateTime.MinValue;
                result.RightModifiedDate = correspondingFolder != string.Empty
                    ? Directory.GetLastWriteTime(correspondingFolder)
                    : DateTime.MinValue;
                result.ExistsLeft = currentFolder != string.Empty;
                result.ExistsRight = correspondingFolder != string.Empty;
            }
            else
            {
                result.RightFilePath = currentFolder;
                result.LeftFilePath = correspondingFolder;
                result.RightCreatedDate = currentFolder != string.Empty
                    ? Directory.GetCreationTime(currentFolder)
                    : DateTime.MinValue;
                result.RightModifiedDate = currentFolder != string.Empty
                    ? Directory.GetLastWriteTime(currentFolder)
                    : DateTime.MinValue;
                result.LeftCreatedDate = correspondingFolder != string.Empty
                    ? Directory.GetCreationTime(correspondingFolder)
                    : DateTime.MinValue;
                result.LeftModifiedDate = correspondingFolder != string.Empty
                    ? Directory.GetLastWriteTime(correspondingFolder)
                    : DateTime.MinValue;
                result.ExistsRight = currentFolder != string.Empty;
                result.ExistsLeft = correspondingFolder != string.Empty;
            }

            result.Match = currentFolder != string.Empty && correspondingFolder != string.Empty;
            result.Compared = true;

            return result;
        }

        private CompareResult ProcessFile(string fileOrFolder, List<string> otherFiles, CompareDirection direction)
        {
            var file = otherFiles.SingleOrDefault(r => r.IsFile() && r.EndsWith('\\' + fileOrFolder.GetFileName()));
            var result = file != null
                ? ProcessFileInternal(fileOrFolder, file, direction)
                : ProcessFileInternal(fileOrFolder, string.Empty, direction);
            return result;
        }

        private CompareResult ProcessFileInternal(string fileOrFolder, string file, CompareDirection direction)
        {
            var result = new CompareResult();
            result.FileName = fileOrFolder.GetFileName();
            result.FileExtension = fileOrFolder.GetFileExtension();

            if (direction == CompareDirection.Left)
            {
                result.LeftFilePath = fileOrFolder;
                result.LeftCreatedDate = File.GetCreationTime(fileOrFolder);
                result.LeftModifiedDate = File.GetLastWriteTime(fileOrFolder);
                result.LeftHash = file != string.Empty ? MD5Hash.HashFile(fileOrFolder) : string.Empty;
                result.RightFilePath = file != string.Empty ? file : string.Empty;
                result.RightCreatedDate = file != string.Empty ? File.GetCreationTime(file) : DateTime.MinValue;
                result.RightModifiedDate = file != string.Empty ? File.GetLastWriteTime(file) : DateTime.MinValue;
                result.RightHash = file != string.Empty ? MD5Hash.HashFile(file) : string.Empty;
                result.ExistsLeft = true;
                result.ExistsRight = file != string.Empty;
            }
            else
            {
                result.RightFilePath = fileOrFolder;
                result.RightCreatedDate = File.GetCreationTime(fileOrFolder);
                result.RightModifiedDate = File.GetLastWriteTime(fileOrFolder);
                result.RightHash = file != string.Empty ? MD5Hash.HashFile(fileOrFolder) : string.Empty;
                result.LeftFilePath = file != string.Empty ? file : string.Empty;
                result.LeftCreatedDate = file != string.Empty ? File.GetCreationTime(file) : DateTime.MinValue;
                result.LeftModifiedDate = file != string.Empty ? File.GetLastWriteTime(file) : DateTime.MinValue;
                result.LeftHash = file != string.Empty ? MD5Hash.HashFile(file) : string.Empty;
                result.ExistsRight = true;
                result.ExistsLeft = file != string.Empty;
            }

            result.Match = file != string.Empty ? result.LeftHash == result.RightHash : false;
            result.Compared = true;
            result.IsFile = true;

            return result;
        }

        private List<CompareResult> ProcessByType(string fileOrFolder, List<string> compareItems,
            CompareDirection direction)
        {
            var _compareItems = compareItems ?? new List<string>();
            var results = new List<CompareResult>();
            if (fileOrFolder.IsFile())
                results.Add(ProcessFile(fileOrFolder, _compareItems, direction));
            else
                results.AddRange(ProcessFolder(fileOrFolder, _compareItems, direction));
            return results;
        }

        private bool IsNotPresent(CompareResult result)
        {
            return !(result.ExistsLeft && result.ExistsRight);
        }

        private List<CompareResult> GetRightOnly(List<CompareResult> results)
        {
            return results.Where(i => i.ExistsLeft != i.ExistsRight).ToList();
        }
    }
}