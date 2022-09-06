using Bev.IO.SpectrumPod;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Bev.IO.SpectrumLoader
{
    public class AsciiReader
    {
        private readonly string[] lines; // the complete file as a text line array
        private readonly string[] signatureTokens = new string[6]; // magic strings for Perkin Elemer ASC-files

        public Spectrum Spectrum { get; private set; }
        public PeFileSignature FileSignature { get; }
        public double PeFileVersion => ParseToDouble(signatureTokens[5]);

        public AsciiReader(string[] textLines)
        {
            lines = textLines;
            Spectrum = new Spectrum();
            signatureTokens = SplitSignatureLine(lines[0]);
            FileSignature = GetFileSignature();
            ParseSpectralData();
            ParseSpectralHeader();
            ParseUnitNames();
        }

        public AsciiReader(LoadSpecFile file) : this(file.LinesInFile)
        {
            Spectrum.SourceFileName = file.FileName;
            Spectrum.SourceFileCreationDate = file.FileCreationTime;
        }

        private string[] SplitSignatureLine(string signature)
        {
            string[] sigTok = new string[6];
            if (signature.Length < 72)
                return sigTok;
            for (int i = 0; i < sigTok.Length; i++)
                sigTok[i] = signature.Substring(i * 12, 12);
            return sigTok;
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
            if (!signatureTokens[0].Contains("PE"))
                return PeFileSignature.InValid;
            if (!signatureTokens[2].Contains("SPECTRUM"))
                return PeFileSignature.InValid;
            if (!signatureTokens[3].Contains("ASCII"))
                return PeFileSignature.InValid;
            if (!signatureTokens[4].Contains("PEDS")) // no idea what this means
                return PeFileSignature.InValid;
            if (signatureTokens[5].Contains("1.60"))
                return PeFileSignature.ValidVer160;
            if (signatureTokens[5].Contains("4.00"))
                return PeFileSignature.ValidVer400;
            return PeFileSignature.Valid;
        }

        private void ParseSpectralHeader()
        {
            if (FileIsInvalid())
                return;
            Spectrum.Type = EstimateTypeOfSpectrum();
            Spectrum.AddMetaData("SourceReference", ExtractLine(2)); // original filename
            Spectrum.MeasurementDate = CreationDate();
            Spectrum.ModificationDate = ModificationDate();
            Spectrum.AddMetaData("Origin", $"Data parsed by {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");
            Spectrum.AddMetaData("Owner", ExtractLine(7));
            Spectrum.AddMetaData("SampleDescription", ExtractLine(8));
            Spectrum.AddMetaData("Title", ExtractLine(8));
            if (FileSignature == PeFileSignature.ValidVer400)
            {
                int offset = GetIndexOfHdr() - 75; // all lines below #15 are shifted by this amount if multiple comments are added
                AddFreeComments(offset);
                Spectrum.AddMetaData("SpectrometerModel", ExtractLine(11));
                Spectrum.AddMetaData("SpectrometerSerialNumber", ExtractLine(12));
                Spectrum.AddMetaData("SpectrometerSystem", EstimateSpectrometerSystem());
                Spectrum.AddMetaData("SoftwareID", ExtractLine(13));
                Spectrum.AddMetaData("InstrumentParameters", ExtractLine(24 + offset));
                Spectrum.AddMetaData("MonochromatorChange", ExtractLine(41 + offset));
                Spectrum.AddMetaData("LampChange", ExtractLine(42 + offset));
                AddDetectorChange(offset);
                AddDetectorGains(offset);
                Spectrum.AddMetaData("CommonBeamMask", ExtractLine(45 + offset));
                Spectrum.AddMetaData("CommonBeamDepolarizer", ExtractLine(46 + offset));
                Spectrum.AddMetaData("Attenuators", ExtractLine(47 + offset));
                Spectrum.AddMetaData("SampleBeamPosition", ExtractLine(44 + offset));
                AddBandPass(offset);
                AddIntegrationTime(offset);
            }
            if (FileSignature == PeFileSignature.ValidVer160)
            {
                // this is just a guess
                Spectrum.AddMetaData("SpectrometerSystem", ExtractLine(20));
            }
        }

        private void AddDetectorGains(int offset)
        {
            string line = ExtractLine(35 + offset);
            string[] tokens = line.Split(new[] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (tokens.Length)
            {
                case 4:
                    Spectrum.AddMetaData("DetectorGain_UVVIS", "auto");
                    Spectrum.AddMetaData("DetectorGain_NIR", tokens[3].Trim());
                    Spectrum.AddMetaData("DetectorGain_IR", tokens[1].Trim());
                    break;
                default:
                    Spectrum.AddMetaData($"DetectorGain", line);
                    break;
            }
        }

        private void AddDetectorChange(int offset)
        {
            AddMultiRegionParameter("DetectorChange", 43 + offset);
        }

        private void AddBandPass(int offset)
        {
            // either 17 or 31
            AddMultiRegionParameter("BandPass", 17 + offset);
        }

        private void AddIntegrationTime(int offset)
        {
            // either 32 or 33
            AddMultiRegionParameter("IntegrationTime", 32 + offset);
        }

        private void AddMultiRegionParameter(string key, int lineIndex)
        {
            string line = ExtractLine(lineIndex);
            string[] tokens = line.Split(new[] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (tokens.Length)
            {
                case 2:
                    Spectrum.AddMetaData($"{key}_UVVIS", tokens[1].Trim());
                    Spectrum.AddMetaData($"{key}_NIR", tokens[0].Trim());
                    break;
                case 4:
                    Spectrum.AddMetaData($"{key}_UVVIS", tokens[3].Trim());
                    Spectrum.AddMetaData($"{key}_NIR", tokens[1].Trim());
                    break;
                case 6:
                    Spectrum.AddMetaData($"{key}_UVVIS", tokens[5].Trim());
                    Spectrum.AddMetaData($"{key}_NIR", tokens[3].Trim());
                    Spectrum.AddMetaData($"{key}_IR", tokens[1].Trim());
                    break;
                default:
                    Spectrum.AddMetaData($"{key}", line);
                    break;
            }
        }

        private void AddFreeComments(int offset)
        {
            var comments = ParseFreeComments(offset);
            if (comments.Length == 0)
                return;
            for (int i = 0; i < comments.Length; i++)
            {
                Spectrum.AddMetaData($"Comment{i + 1}", comments[i]);
            }
        }

        private string[] ParseFreeComments(int offset)
        {
            List<string> comments = new List<string>();
            for (int i = 0; i < offset; i++)
            {
                comments.Add(ExtractLine(i + 14));
            }
            return comments.ToArray();
        }

        private SpectralType EstimateTypeOfSpectrum()
        {
            if (signatureTokens[0].Contains("PE UV"))
                return SpectralType.UvVis;
            if (signatureTokens[0].Contains("PE FL")) // fluorescence?
                return SpectralType.UvVis;
            if (signatureTokens[0].Contains("PE IR"))
                return SpectralType.Infrared;
            return SpectralType.Unknown;
        }

        private string EstimateSpectrometerSystem()
        {
            string snPrefix = " SN:";
            if (string.IsNullOrWhiteSpace(ExtractLine(12)))
                snPrefix = string.Empty;
            return $"{ExtractLine(11)}{snPrefix}{ExtractLine(12)}".Trim();
        }

        private void ParseUnitNames()
        {
            if (FileIsInvalid())
                return;
            int index = GetIndexOfUnits();
            Spectrum.SetUnitNames(ExtractLine(index + 1), ExtractLine(index + 2));
            // this is for debug puposes only
            double value1 = ParseToDouble(ExtractLine(index + 3)); // unknown (1.0)
            double value2 = ParseToDouble(ExtractLine(index + 4)); // unknown (0.0)
            double value3 = ParseToDouble(ExtractLine(index + 5)); // FirstX of source data
            double value4 = ParseToDouble(ExtractLine(index + 6)); // DeltaX of source data
            double value5 = ParseToDouble(ExtractLine(index + 7)); // number of points
            double value6 = ParseToDouble(ExtractLine(index + 8)); // unknown (8)
            double value7 = ParseToDouble(ExtractLine(index + 9)); // MaxY
            double value8 = ParseToDouble(ExtractLine(index + 10));// MinY
            //Console.WriteLine($"#GR - v1:{value1} v2:{value2} v6:{value6}");
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
                if (tupel.IsValid) Spectrum.AddDataValue(tupel);
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

        private string ExtractLine(int lineNumber) // lineNumber is zero-based! 
        {
            if (lines == null) return string.Empty;
            if (lineNumber >= lines.Length) return string.Empty;
            if (lineNumber < 0) return string.Empty;
            return lines[lineNumber].Trim(); //TODO really trimming?
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
                case PeFileSignature.NoFile:
                case PeFileSignature.InValid:
                    return true;
                case PeFileSignature.ValidVer400:
                case PeFileSignature.ValidVer160:
                case PeFileSignature.Valid:
                    return false;
            }
            return true;
        }

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
