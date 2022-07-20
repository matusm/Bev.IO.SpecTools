using Bev.IO.FileLoader;
using Bev.IO.PerkinElmerAsciiReader;
using Bev.IO.SpectrumPod;
using System;
using System.Globalization;
using System.IO;

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
                Spectrum spec = ProcessFile(fn);
            }

        }

        private static Spectrum ProcessFile(string filename)
        {
            SpectralFile sFile = new SpectralFile(filename);
            AsciiReader aReader = new AsciiReader(sFile.LinesInFile);
            Spectrum spectrum = aReader.Spectrum;

            Console.WriteLine($"Filename:              {filename}");
            Console.WriteLine($"File signature:        {aReader.FileSignature}");
            Console.WriteLine($"spectrum.AbscissaType: {spectrum.AbscissaType}");
            Console.WriteLine($"Spectrum.FirstX:       {spectrum.FirstX}");
            Console.WriteLine($"Spectrum.LastX:        {spectrum.LastX}");
            Console.WriteLine($"Spectrum.DeltaX:       {spectrum.DeltaX}");
            Console.WriteLine($"Spectrum.Length:       {spectrum.Length}");
            Console.WriteLine($"Spectrum.MaxY:         {spectrum.MaxY}");
            Console.WriteLine($"Spectrum.MinY:         {spectrum.MinY}");
            Console.WriteLine($"Spectrum.FirstY:       {spectrum.FirstY}");
            Console.WriteLine($"Spectrum.LastY:        {spectrum.LastY}");
            Console.WriteLine($"Spectrum.XUnitName:    {spectrum.XUnitName}");
            Console.WriteLine($"Spectrum.YUnitName:    {spectrum.YUnitName}");
            Console.WriteLine($"MeasurementDate:       {spectrum.Header.MeasurementDate}");
            Console.WriteLine($"ModificationDate:      {spectrum.Header.ModificationDate}");
            Console.WriteLine($"Origin:                {spectrum.Header.Origin}");
            Console.WriteLine($"Owner:                 {spectrum.Header.Owner}");
            Console.WriteLine($"SampleDescription:     {spectrum.Header.SampleDescription}");
            Console.WriteLine($"SpectrometerModel:     {spectrum.Header.SpectrometerModel}");
            Console.WriteLine($"SpectrometerSN:        {spectrum.Header.SpectrometerSerialNumber}");
            Console.WriteLine($"SpectrometerSystem:    {spectrum.Header.SpectrometerSystem}");
            Console.WriteLine($"Title:                 {spectrum.Header.Title}");


            Console.WriteLine("=========================================================");

            return spectrum;
        }
    }
}
