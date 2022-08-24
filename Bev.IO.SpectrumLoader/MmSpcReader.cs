using Bev.IO.SpectrumPod;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Bev.IO.SpectrumLoader
{
    public class MmSpcReader
    {
        private readonly string[] lines; // the complete file as a text line array
        public Spectrum Spectrum { get; private set; }

        public MmSpcReader(string[] textLines)
        {
            lines = textLines;
            Spectrum = new Spectrum();
            ParseSpectralData();
            ParseSpectralHeader();
        }

        private void ParseSpectralHeader()
        {
            Spectrum.Type = EstimateTypeOfSpectrum();
            Spectrum.AddMetaData("Origin", $"Data parsed by {Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");
            Spectrum.AddMetaData("SampleDescription", ExtractLine(1));
            //Spectrum.Header.FreeComments = GetFreeComments(2, GetIndexOfData());
        }

        private void ParseSpectralData()
        {
            int startIndex = GetIndexOfData() + 1;
            if (startIndex >= lines.Length)
                return;
            for (int i = startIndex; i < lines.Length; i++)
            {
                SpectralPoint tupel = ParseToTupel(lines[i]);
                if (tupel.IsValid) Spectrum.AddDataValue(tupel);
            }
        }

        private string[] GetFreeComments(int fromIndex, int toIndex)
        {
            List<string> commentLines = new List<string>();
            for (int i = fromIndex; i < toIndex; i++)
            {
                commentLines.Add(ExtractLine(i));
            }
            return commentLines.ToArray();
        }

        private SpectralType EstimateTypeOfSpectrum()
        {
            if (lines[0].Contains("**** UV/VIS ****"))
                return SpectralType.UvVis;
            if (lines[0].Contains("**** UV/VIS ****"))
                return SpectralType.Infrared;
            if (lines[0].Contains("**** RAMAN ****"))
                return SpectralType.Raman;
            return SpectralType.Unknown;
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

        private int GetIndexOfData() => GetIndexOfKey("@@@@"); // legacy PL2 separator

        private int GetIndexOfKey(string keyword)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(keyword))
                    return i;
            }
            return -1; //TODO
        }

        private string ExtractLine(int lineNumber) // lineNumber is zero-based! 
        {
            if (lines == null) return string.Empty;
            if (lineNumber >= lines.Length) return string.Empty;
            if (lineNumber < 0) return string.Empty;
            return lines[lineNumber].Trim(); //TODO really trimming?
        }

    }
}
