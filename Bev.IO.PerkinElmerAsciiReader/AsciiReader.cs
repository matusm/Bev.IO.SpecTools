using Bev.IO.SpectrumPod;
using System;
using System.Globalization;

namespace Bev.IO.PerkinElmerAsciiReader
{
    public class AsciiReader
    {
        public Spectrum Spectrum { get; private set; }
        public PeFileSignature FileSignature { get; }

        public AsciiReader(string[] textLines)
        {
            lines = textLines;
            Spectrum = new Spectrum();
            FileSignature = GetFileSignature();
            ParseSpectralData();
            ParseSpectralHeader();
            ParseParameters();
        }

        private PeFileSignature GetFileSignature()
        {
            int minFileLength = 10;
            if (lines == null)
                return PeFileSignature.NoFile;
            if (lines.Length < minFileLength)
                return PeFileSignature.InValid;
            int iHdr = GetIndexOfHdr();
            int iUnits = GetIndexOfUnits();
            int iData = GetIndexOfData();
            if ((iUnits - iHdr) != 3)
                return PeFileSignature.InValid;
            if ((iData - iUnits) != 11)
                return PeFileSignature.InValid;
            // check magic bytes
            if(!lines[0].Contains("PE"))
                return PeFileSignature.InValid;
            if (!lines[0].Contains("ASCII"))
                return PeFileSignature.InValid;
            if (!lines[0].Contains("SPECTRUM"))
                return PeFileSignature.InValid;
            if (!lines[0].Contains("PEDS"))
                return PeFileSignature.InValid;
            if (lines[0].Contains("1.60"))
                return PeFileSignature.ValidVer160;
            if (lines[0].Contains("4.00"))
                return PeFileSignature.ValidVer400;
            return PeFileSignature.Valid;
        }

        private void ParseSpectralHeader()
        {
            if (FileIsInvalid())
                return;
            Spectrum.Header.MeasurementDate = CreationDate();
            Spectrum.Header.ModificationDate = ModificationDate();
            Spectrum.Header.Owner = ExtractLine(7);
            Spectrum.Header.SampleDescription = ExtractLine(8);
            Spectrum.Header.SpectrometerModel = ExtractLine(11);
            Spectrum.Header.SpectrometerSerialNumber = ExtractLine(12);
            Spectrum.Header.SoftwareID = ExtractLine(13);
            Spectrum.Header.SpectrometerSystem = EstimateSpectrometerSystem();
        }

        private string EstimateSpectrometerSystem()
        {
            string conjunction = " SN:";
            if (string.IsNullOrWhiteSpace(Spectrum.Header.SpectrometerSerialNumber))
                conjunction = string.Empty;
            return $"{Spectrum.Header.SpectrometerModel}{conjunction}{Spectrum.Header.SpectrometerSerialNumber}".Trim();
        }

        private void ParseParameters()
        {
            if (FileIsInvalid())
                return;
            int index = GetIndexOfUnits();
            Spectrum.SetUnitNames(ExtractLine(index + 1), ExtractLine(index + 2));
            // this is for test puposes only
            double value1 = ParseToDouble(ExtractLine(index + 3)); // unknown (1.0)
            double value2 = ParseToDouble(ExtractLine(index + 4)); // unknown (0.0)
            double value3 = ParseToDouble(ExtractLine(index + 5)); // FirstX of source data
            double value4 = ParseToDouble(ExtractLine(index + 6)); // DeltaX of source data
            double value5 = ParseToDouble(ExtractLine(index + 7)); // number of points
            double value6 = ParseToDouble(ExtractLine(index + 8)); // unknown (8)
            double value7 = ParseToDouble(ExtractLine(index + 9)); // MaxY
            double value8 = ParseToDouble(ExtractLine(index + 10));// MinY
            Console.WriteLine($"#GR - v1:{value1} v2:{value2} v6:{value6}");
        }

        private void ParseSpectralData()
        {
            if (FileIsInvalid()) 
                return;
            int startIndex = GetIndexOfData() + 1;
            if (startIndex >= lines.Length)
                return;
            for (int i = startIndex; i < lines.Length; i++)
            {
                SpectralPoint tupel = ParseToTupel(lines[i]);
                if (tupel.IsValid) Spectrum.AddValue(tupel);
            }
        }

        private DateTime CreationDate()
        {
            string s = $"{ExtractLine(3)} {ExtractLine(4)}";
            string format = "yy/MM/dd HH:mm:ss.ff";
            try
            {
                return DateTime.ParseExact(s, format, null);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private DateTime ModificationDate()
        {
            string s = $"{ExtractLine(5)} {ExtractLine(6)}";
            string format = "yy/MM/dd HH:mm:ss.ff";
            try
            {
                return DateTime.ParseExact(s, format, null);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private SpectralPoint ParseToTupel(string dataLine)
        {
            string[] tokens = dataLine.Split(new[] { ' ', '=', ';', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 2)
                return new SpectralPoint(double.NaN, double.NaN);
            double x = ParseToDouble(tokens[0]);
            double y = ParseToDouble(tokens[1]);
            return new SpectralPoint(x, y);
        }

        private double ParseToDouble(string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                return result;
            return double.NaN;
        }

        private string ExtractLine(int lineNumber)
        {
            if (lines == null) return string.Empty;
            if (lineNumber >= lines.Length) return string.Empty;
            return lines[lineNumber].Trim(); //TODO really?
        }

        private int GetIndexOfData() => GetIndexOfKey("#DATA");

        private int GetIndexOfUnits() => GetIndexOfKey("#GR");

        private int GetIndexOfHdr() => GetIndexOfKey("#HDR");

        private int GetIndexOfKey(string keyword)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(keyword))
                    return i;
            }
            return -1; //TODO
        }

        private bool FileIsInvalid()
        {
            switch (FileSignature)
            {
                case PeFileSignature.Unknown:
                    return true;
                case PeFileSignature.NoFile:
                    return true;
                case PeFileSignature.InValid:
                    return true;
                case PeFileSignature.ValidVer400:
                    return false;
                case PeFileSignature.ValidVer160:
                    return false;
                case PeFileSignature.Valid:
                    return false;
            }
            return true;
        }

        private string[] lines;
    }

    public enum PeFileSignature
    {
        Unknown,
        NoFile,
        InValid,
        ValidVer400,
        ValidVer160,
        Valid
    }

}
