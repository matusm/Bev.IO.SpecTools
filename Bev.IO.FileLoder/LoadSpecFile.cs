using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Bev.IO.FileLoader
{
    public class LoadSpecFile
    {
        public string[] LinesInFile { get; private set; }
        public string FileName { get; }
        public DateTime FileCreationTime { get; }

        public LoadSpecFile(string path) : this(path, Encoding.Default) { }

        public LoadSpecFile(string path, Encoding encoding)
        {
            FileName = Path.GetFileName(path);
            FileCreationTime = File.GetCreationTimeUtc(path);
            //Encoding.GetEncoding(437) for MS-DOS
            LoadFile(path, encoding);
        }

        private void LoadFile(string path, Encoding encoding)
        {
            try
            {
                string allText = File.ReadAllText(path, encoding);
                if (string.IsNullOrWhiteSpace(allText))
                    return;
                LinesInFile = Regex.Split(allText, "\r\n|\r|\n");
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
