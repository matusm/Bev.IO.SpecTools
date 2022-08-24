using Bev.IO.SpectrumPod;
using System.Globalization;
using System.Text;

namespace Bev.IO.JcampDxWriter
{
    public class JcampWriter
    {
        private readonly Spectrum spectrum;
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public double Xfactor = 1;
        public double Yfactor = 1;

        public JcampWriter(Spectrum spectrum)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            this.spectrum = spectrum;
        }

        public string GetRecord()
        {
            ConsolidateRecords();
            stringBuilder.Clear();
            CreateHeader();
            CreateData();
            return stringBuilder.ToString();
        }

        private void CreateHeader() => stringBuilder.Append(spectrum.MetaDataJcamp);

        private void CreateData() // works only for factors = 1!
        {
            if (spectrum.AbscissaType == SpectralSpacing.FixedSpacing)
            {
                CoreRecord("XYDATA", "(X++(Y..Y))");
                foreach (var point in spectrum.Data)
                {
                    stringBuilder.AppendLine(point.ToLine(" "));
                    //stringBuilder.AppendLine($"{point.X / Xfactor:F6} {point.Y / Yfactor:F6}");
                }
            }
            if (spectrum.AbscissaType == SpectralSpacing.VariableSpacing)
            {
                CoreRecord("XYPOINTS", "(XY..XY)");
                foreach (var point in spectrum.Data)
                {
                    stringBuilder.AppendLine(point.ToLine(", "));
                    //stringBuilder.AppendLine($"{point.X / Xfactor:F6}, {point.Y / Yfactor:F6}");
                }
            }
            CoreRecord("END", string.Empty);
        }

        private void ConsolidateRecords()
        {
            spectrum.AddMetaData("XUnits", TranslateUnitForJcamp(spectrum.XUnitName));
            spectrum.AddMetaData("YUnits", TranslateUnitForJcamp(spectrum.YUnitName));
            spectrum.AddMetaData("XFactor", Xfactor.ToString());
            spectrum.AddMetaData("YFactor", Yfactor.ToString());
        }

        private void CoreRecord(string dataLabelName, string dataSet)
        {
            HeaderRecord hr = new HeaderRecord(dataSet, true, true);
            hr.PlainLabel = dataLabelName;
            stringBuilder.AppendLine(hr.ToJcampString(-1));
        }

        private string TranslateUnitForJcamp(string unitSymbol)
        {
            string symbol = unitSymbol.ToUpper().Trim();
            if(string.IsNullOrWhiteSpace(symbol))
               return "ARBITRARY UNITS";
            switch (symbol)
            {
                case "NM":
                    return "NANOMETERS";
                case "CM-1":
                case "1/CM":
                case "WN":
                    return "1/CM";
                case "A":
                case "ABS":
                    return "ABSORBANCE";
                case "UM":
                case "µM":
                    return "MICROMETERS";
                case "S":
                case "SEC":
                    return "SECONDS";
                case "%T":
                case "T":
                    return "TRANSMITTANCE";
                case "%R":
                case "R":
                    return "REFLECTANCE";
                case "INT":
                    return "INTENSITY"; // really?
                case "EGY":
                    return "ENERGY"; // really?
                default:
                    return symbol;
            }
        }
    }
}
