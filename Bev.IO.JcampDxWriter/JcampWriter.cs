using Bev.IO.SpectrumPod;
using System;
using System.Globalization;
using System.Text;

namespace Bev.IO.JcampDxWriter
{
    public class JcampWriter
    {
        private const string dataLabelFlag = "##";
        private const string dataLabelTerminator = "= ";    // trailing space included
        private const string tabularIndend = "";
        private const int maxColumns = 80;
        private Spectrum spectrum;

        public string Xunits = string.Empty;
        public string Yunits = string.Empty;
        public string Xlabel = string.Empty;
        public string Ylabel = string.Empty;

        public double Xfactor = 1;
        public double Yfactor = 1;

        public JcampWriter(Spectrum spectrum)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            this.spectrum = spectrum;
        }

        public string GetDataRecords()
        {
            StringBuilder sb = new StringBuilder();
            AppendRecord("TITLE", spectrum.Header.Title);
            AppendRecord("JCAMP-DX", "4.24");
            AppendRecord("DATA TYPE", spectrum.Header.DataType);
            AppendRecord("SAMPLE DESCRIPTION", spectrum.Header.SampleDescription);
            AppendRecord("ORIGIN", spectrum.Header.Origin);
            AppendRecord("OWNER", spectrum.Header.Owner);
            AppendRecord("CLASS", spectrum.Header.Class);
            AppendRecord("SOURCE REFERENCE", spectrum.Header.SourceReference);
            AppendRecord("CROSS REFERENCE", spectrum.Header.CrossReference);
            AppendRecord("SPECTROMETER/DATA SYSTEM", spectrum.Header.SpectrometerSystem);
            AppendRecord("INSTRUMENT PARAMETERS", spectrum.Header.InstrumentParameters);
            AppendRecord("DATE", spectrum.Header.MeasurementDate.ToString("yy/MM/dd"));
            AppendRecord("TIME", spectrum.Header.MeasurementDate.ToString("HH:mm:ss"));
            AppendRecord("LONG DATE", spectrum.Header.MeasurementDate.ToString("yyyy/MM/dd HH:mm:ssK")); // TODO this is not 4.24 compliant!
            AppendRecord("NPOINTS", spectrum.Length.ToString());
            AppendRecord("XUNITS", TranslateUnit(spectrum.XUnitName));
            AppendRecord("YUNITS", TranslateUnit(spectrum.YUnitName));
            AppendNumRecord("FIRSTX", spectrum.FirstX);
            AppendNumRecord("FIRSTY", spectrum.FirstY);
            AppendNumRecord("LASTX", spectrum.LastX);
            AppendNumRecord("DELTAX", spectrum.DeltaX);
            AppendNumRecord("MINX", spectrum.MinX);
            AppendNumRecord("MAXX", spectrum.MaxX);
            AppendNumRecord("MINY", spectrum.MinY);
            AppendNumRecord("MAXY", spectrum.MaxY);
            AppendNumRecord("XFACTOR", Xfactor);
            AppendNumRecord("YFACTOR", Yfactor);
            AppendRecord("XLABEL", Xlabel);
            AppendRecord("YLABEL", Ylabel);
            // here comes the actual data
            if (spectrum.AbscissaType == SpectralSpacing.FixedSpacing)
            {
                AppendRecord("XYDATA", "(X++(Y..Y))");
                foreach (var point in spectrum.Data)
                {
                    sb.AppendLine($"{tabularIndend}{point.X / Xfactor} {point.Y / Yfactor}");
                }
            }
            if (spectrum.AbscissaType == SpectralSpacing.VariableSpacing)
            {
                AppendRecord("XYPOINTS", "(XY..XY)");
                foreach (var point in spectrum.Data)
                {
                    sb.AppendLine($"{tabularIndend}{point.X / Xfactor} , {point.Y / Yfactor}");
                }
            }
            sb.AppendLine(LabeledDataRecord("END", string.Empty)); // cant use AppendRecord() here !
            return sb.ToString();

            void AppendNumRecord(string dataLabelName, double dataNum)
            {
                if (double.IsNaN(dataNum))
                    return;
                AppendRecord(dataLabelName, dataNum.ToString());
            }

            void AppendRecord(string dataLabelName, string dataSet)
            {
                if (string.IsNullOrEmpty(dataSet))
                    return;
                sb.AppendLine(LabeledDataRecord(dataLabelName, dataSet));
            }

        }

        private string LabeledDataRecord(string dataLabelName, string dataSet) => TruncateString($"{dataLabelFlag}{dataLabelName}{dataLabelTerminator}{dataSet}");

        private string TruncateString(string longString)
        {
            if (string.IsNullOrEmpty(longString))
                return longString;
            if (longString.Length <= maxColumns)
                return longString;
            return $"{longString.Substring(0, maxColumns - 3)}...";
        }

        private string TranslateUnit(string unitSymbol)
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
                    return "1/CM";
                case "A":
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
