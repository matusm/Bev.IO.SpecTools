using Bev.IO.FileLoader;
using Bev.IO.PerkinElmerAsciiReader;
using Bev.IO.SpectrumPod;
using Bev.IO.MmSpcReader;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace SpecConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            string workingDirectory = Directory.GetCurrentDirectory();
            string[] filenames = Directory.GetFiles(workingDirectory, @"*.asc");
            Array.Sort(filenames);

            foreach (string fn in filenames)
            {
                //Spectrum spectrum = ProcessMmSpcFile(fn);
                Spectrum spectrum = ProcessAsciiFile(fn);
                //CsvWriter writer = new CsvWriter(spectrum);
                //JcampWriter writer = new JcampWriter(spectrum);
                MmSpcWriter writer = new MmSpcWriter(spectrum);
                Console.WriteLine(writer.GetRecord());
            }

        }

        private static Spectrum ProcessMmSpcFile(string filename)
        {
            LoadSpecFile sFile = new LoadSpecFile(filename, Encoding.GetEncoding(437));
            MmSpcReader sReader = new MmSpcReader(sFile.LinesInFile);
            Spectrum spectrum = sReader.Spectrum;
            spectrum.SourceFileName = sFile.FileName;
            spectrum.SourceFileCreationDate = sFile.FileCreationTime;
            return spectrum;
        }

        private static Spectrum ProcessAsciiFile(string filename)
        {
            LoadSpecFile sFile = new LoadSpecFile(filename);
            AsciiReader aReader = new AsciiReader(sFile);
            Spectrum spectrum = aReader.Spectrum;
            return spectrum;
        }
    }
}
