using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Bev.IO.FileLoader
{
    public class LoadSpecFile
    {
        public string[] LinesInFile { get; private set; }
        public string Filename { get; private set; }

        public LoadSpecFile(string filename) : this(filename, Encoding.Default) { }

        public LoadSpecFile(string filename, Encoding encoding)
        {
            //Encoding.GetEncoding(437) for MS-DOS
            LoadFile(filename, encoding);
        }

        private void LoadFile(string filename, Encoding encoding)
        {
            try
            {
                string allText = File.ReadAllText(filename, encoding);
                if (string.IsNullOrWhiteSpace(allText))
                {
                    return;
                }
                LinesInFile = Regex.Split(allText, "\r\n|\r|\n");
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
