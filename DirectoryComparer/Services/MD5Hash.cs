// Thanks to http://stackoverflow.com/a/2150474/312219

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DirectoryComparer.Services
{
    public class MD5Hash
    {
        public static string HashFile(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return HashFile(fs);
            }
        }

        public static string HashFile(FileStream stream)
        {
            var sb = new StringBuilder();

            if (stream != null)
            {
                stream.Seek(0, SeekOrigin.Begin);

                var md5 = MD5.Create();
                var hash = md5.ComputeHash(stream);
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));

                stream.Seek(0, SeekOrigin.Begin);
            }

            return sb.ToString();
        }
    }
}