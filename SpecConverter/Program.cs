using Bev.IO.FileLoader;
using Bev.IO.PerkinElmerAsciiReader;
using Bev.IO.SpectrumPod;
using Bev.IO.JcampDxWriter;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace SpecConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version;

            string workingDirectory = Directory.GetCurrentDirectory();
            string[] filenames = Directory.GetFiles(workingDirectory, @"*.asc");
            Array.Sort(filenames);

            foreach (string fn in filenames)
            {
                Spectrum spec = ProcessFile(fn);
                spec.Header.DataType = "UV/VIS SPECTRUM";
                spec.Header.Title = spec.Header.SampleDescription;
                spec.Header.SourceReference = Path.GetFileName(fn);
                spec.Header.Origin = $"{appName} {appVersion}";
                spec.Header.XLabel = "Wavelength / nm";
                spec.Header.YLabel = "Transmittance / %";

                JcampWriter jw = new JcampWriter(spec);
                Console.WriteLine(jw.GetDataRecords());

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
            Console.WriteLine();
            Console.WriteLine("=========================================================");

            return spectrum;
        }
    }
}
