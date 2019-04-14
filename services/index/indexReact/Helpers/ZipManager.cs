using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace indexReact.Helpers
{
    public static class ZipManager
    {
        private const long MaxSize = 1024 * 1024 * 4;

        public static string[] Unzip(Stream file)
        {
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                var totalLen = zip.Entries.Sum(e => e.Length);
                if (totalLen > MaxSize)
                    throw new Exception("toBig");
                return zip.Entries.Select(e => e.FullName).ToArray();
            }
        }
    }
}