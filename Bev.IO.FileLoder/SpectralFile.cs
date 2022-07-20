using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Bev.IO.FileLoader
{
    public class SpectralFile
    {
        public string[] LinesInFile { get; private set; }

        public SpectralFile(string filename)
        {
            LoadFile(filename);
        }

        private void LoadFile(string filename)
        {
            try
            {
                string allText = File.ReadAllText(filename);
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
