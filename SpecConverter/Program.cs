using Bev.IO.SpectrumPod;
using Bev.IO.SpectrumLoader;
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
            //string[] filenames = Directory.GetFiles(workingDirectory, @"*.asc");
            string[] filenames = { "Sample44.Sample_05.asc", "IR.asc" };
            Array.Sort(filenames);

            foreach (string fn in filenames)
            {
                //Spectrum spectrum = ProcessMmSpcFile(fn);
                Spectrum spectrum = LoadAsciiFile(fn);
                //CsvWriter writer = new CsvWriter(spectrum);
                //JcampWriter writer = new JcampWriter(spectrum);
                MmSpcWriter writer = new MmSpcWriter(spectrum);
                Console.WriteLine(writer.GetRecord());
            }

        }

        private static Spectrum LoadMmSpcFile(string filename)
        {
            LoadSpecFile sFile = new LoadSpecFile(filename, Encoding.GetEncoding(437));
            MmSpcReader sReader = new MmSpcReader(sFile.LinesInFile);
            Spectrum spectrum = sReader.Spectrum;
            spectrum.SourceFileName = sFile.FileName;
            spectrum.SourceFileCreationDate = sFile.FileCreationTime;
            return spectrum;
        }

        private static Spectrum LoadAsciiFile(string filename)
        {
            LoadSpecFile sFile = new LoadSpecFile(filename);
            AsciiReader aReader = new AsciiReader(sFile);
            Spectrum spectrum = aReader.Spectrum;
            return spectrum;
        }
    }
}
