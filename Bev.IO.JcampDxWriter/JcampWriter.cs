using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bev.IO.JcampDxWriter
{
    public class JcampWriter
    {

        private const string dataLabelFlag = "##";
        private const string dataLabelTerminator = "= ";    // trailing space included
        private const string tabularIndend = "   ";
        private const int maxColumns = 80;
        private Spectrum spectrum;

        public string Title = string.Empty;
        public string DataType = string.Empty;
        public string Origin = string.Empty;
        public string Owner = string.Empty;
        public string SpectrometerSystem = string.Empty;
        public string InstrumentParameters = string.Empty;
        public string SampleDescription = string.Empty;
        public string Concentrations = string.Empty;
        public string SamplingProcedure = string.Empty;
        public string State = string.Empty;
        public string PathLength = string.Empty;
        public string Pressure = string.Empty;
        public string Temperature = string.Empty;
        public string DataProcessing = string.Empty;
        public string SourceReference = string.Empty;
        public string CrossReference = string.Empty;
        public string Class = string.Empty;

        public string Xunits = string.Empty;
        public string Yunits = string.Empty;
        public string Xlabel = string.Empty;
        public string Ylabel = string.Empty;

        public DateTime MeasurementDate;

        public SpectralSpacing AbscissaType = SpectralSpacing.Unknown;
        public int Npoints;
        public double DeltaX = double.NaN;
        public double Xfactor = 1;
        public double Yfactor = 1;
        public double FirstX = double.NaN;
        public double LastX = double.NaN;
        public double FirstY = double.NaN;
        public double MaxX = double.NaN;
        public double MinX = double.NaN;
        public double MaxY = double.NaN;
        public double MinY = double.NaN;

        public SimpleJcampData()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        public void SetSpectrum(Spectrum sortedSpectrum)
        {
            spectrum = sortedSpectrum;
            Npoints = spectrum.Length;
            FirstX = spectrum.FirstX;
            LastX = spectrum.LastX;
            FirstY = spectrum.FirstY;
            MaxX = spectrum.MaxX;
            MinX = spectrum.MinX;
            MaxY = spectrum.MaxY;
            MinY = spectrum.MinY;
            AbscissaType = spectrum.AbscissaType;
            if (spectrum.AbscissaType == SpectralSpacing.FixedSpacing)
                DeltaX = spectrum.DeltaX;
            else
                DeltaX = double.NaN;
            Xunits = spectrum.XUnitName;
            Yunits = spectrum.YUnitName;
        }

        public string GetDataRecords()
        {
            StringBuilder sb = new StringBuilder();
            AppendRecord("TITLE", Title);
            AppendRecord("JCAMP-DX", "4.24");
            AppendRecord("DATA TYPE", DataType);
            AppendRecord("SAMPLE DESCRIPTION", SampleDescription);
            AppendRecord("ORIGIN", Origin);
            AppendRecord("OWNER", Owner);
            AppendRecord("CLASS", Class);
            AppendRecord("SOURCE REFERENCE", SourceReference);
            AppendRecord("CROSS REFERENCE", CrossReference);
            AppendRecord("SPECTROMETER/DATA SYSTEM", SpectrometerSystem);
            AppendRecord("INSTRUMENT PARAMETERS", InstrumentParameters);
            AppendRecord("DATE", MeasurementDate.ToString("yy/MM/dd"));
            AppendRecord("TIME", MeasurementDate.ToString("HH:mm:ss"));
            AppendRecord("LONG DATE", MeasurementDate.ToString("yyyy/MM/dd HH:mm:ssK")); // TODO this is not 4.24 compliant!
            AppendRecord("NPOINTS", Npoints.ToString());
            AppendRecord("XUNITS", Xunits);
            AppendRecord("YUNITS", Yunits);
            AppendNumRecord("FIRSTX", FirstX);
            AppendNumRecord("FIRSTY", FirstY);
            AppendNumRecord("LASTX", LastX);
            AppendNumRecord("DELTAX", DeltaX);
            AppendNumRecord("MINX", MinX);
            AppendNumRecord("MAXX", MaxX);
            AppendNumRecord("MINY", MinY);
            AppendNumRecord("MAXY", MaxY);
            AppendNumRecord("XFACTOR", Xfactor);
            AppendNumRecord("YFACTOR", Yfactor);
            AppendRecord("XLABEL", Xlabel);
            AppendRecord("YLABEL", Ylabel);
            if (spectrum.AbscissaType == SpectralSpacing.FixedSpacing)
            {
                AppendRecord("XYDATA", "(X++(Y..Y))");
                foreach (var point in spectrum.GetSpectralData())
                {
                    sb.AppendLine($"{tabularIndend}{point.X / Xfactor} {point.Y / Yfactor}");
                }
            }
            if (spectrum.AbscissaType == SpectralSpacing.VariableSpacing)
            {
                AppendRecord("XYPOINTS", "(XY..XY)");
                foreach (var point in spectrum.GetSpectralData())
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

    }
}
