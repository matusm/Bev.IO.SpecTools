using Bev.IO.SpectrumPod;
using Bev.IO.SpectrumLoader;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using Bev.IO.PerkinElmerSP;

namespace SpecConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            string workingDirectory = Directory.GetCurrentDirectory();
            string[] filenames = Directory.GetFiles(workingDirectory, @"*.sp");
            //string[] filenames = { "MM04_008.Sample.ASC", "MM04_009.Sample.ASC", "Si3N4_2020.asc" };
            Array.Sort(filenames);

            foreach (string fn in filenames)
            {
                Spectrum spectrum = LoadSpFile(fn);
                WriteSpcFile(spectrum, fn);
                //WriteCsvFile(spectrum, fn);
                //WriteJcampFile(spectrum, fn);
                Console.WriteLine($"{fn} - done.");
            }

        }

        private static void WriteSpcFile(Spectrum spectrum, string fileName)
        {
            string outFileName = Path.ChangeExtension(fileName, ".spc");
            MmSpcWriter writer = new MmSpcWriter(spectrum);
            WriteToFile(outFileName, writer.GetRecord());
        }

        private static void WriteCsvFile(Spectrum spectrum, string fileName)
        {
            string outFileName = Path.ChangeExtension(fileName, ".csv");
            CsvWriter writer = new CsvWriter(spectrum);
            WriteToFile(outFileName, writer.GetRecord());
        }

        private static void WriteJcampFile(Spectrum spectrum, string fileName)
        {
            string outFileName = Path.ChangeExtension(fileName, ".jdx");
            JcampWriter writer = new JcampWriter(spectrum);
            WriteToFile(outFileName, writer.GetRecord());
        }

        private static void WriteToFile(string outFileName, string data)
        {
            // check if data present
            if (string.IsNullOrWhiteSpace(data))
                return;
            // write the file
            try
            {
                StreamWriter hOutFile = File.CreateText(outFileName);
                hOutFile.Write(data);
                hOutFile.Close();
            }
            catch (Exception)
            {
                return;
            }
            return;
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

        private static Spectrum LoadSpFile(string filename)
        {
            SpReader spReader = new SpReader(filename);
            spReader.DebugOutput();
            return spReader.Spectrum;
        }
    }
}
