using DirectoryComparer.Objects;

namespace DirectoryComparer.Services
{
    public static class CompareResultHelper
    {
        public static string GetFileOrFolderName(this CompareResult item)
        {
            if (item.FileName != string.Empty)
                return item.FileName;

            var folderPath = item.LeftFilePath != string.Empty ? item.LeftFilePath : item.RightFilePath;
            return folderPath.GetCurrentDir();
        }
    }
}