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
        private readonly Spectrum spectrum;
        private StringBuilder stringBuilder;


        public double Xfactor = 1;
        public double Yfactor = 1;
        public bool TruncateLines = false;

        public JcampWriter(Spectrum spectrum)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            this.spectrum = spectrum;
            stringBuilder = new StringBuilder();
        }

        public string GetRecord()
        {
            stringBuilder.Clear();
            CreateJcampHeader();
            CreateJcampData();
            return stringBuilder.ToString();
        }

        private void OptionalFreeCommentsRecord()
        {
            if (spectrum.Header.FreeComments.Length == 0)
                return;
            for (int i = 0; i < spectrum.Header.FreeComments.Length; i++)
                OptionalRecord($"$COMMENT_{i}", spectrum.Header.FreeComments[i]);
        }

        private void CreateJcampData()
        {
            if (spectrum.AbscissaType == SpectralSpacing.FixedSpacing)
            {
                CoreRecord("XYDATA", "(X++(Y..Y))");
                foreach (var point in spectrum.Data)
                {
                    stringBuilder.AppendLine($"{tabularIndend}{point.X / Xfactor} {point.Y / Yfactor}");
                }
            }
            if (spectrum.AbscissaType == SpectralSpacing.VariableSpacing)
            {
                CoreRecord("XYPOINTS", "(XY..XY)");
                foreach (var point in spectrum.Data)
                {
                    stringBuilder.AppendLine($"{tabularIndend}{point.X / Xfactor}, {point.Y / Yfactor}");
                }
            }
            CoreRecord("END", string.Empty);
        }

        private void CreateJcampHeader()
        {
            CoreRecord("TITLE", spectrum.Header.Title);
            CoreRecord("JCAMP-DX", "4.24");
            CoreRecord("DATA TYPE", spectrum.Header.DataType);
            CoreRecord("ORIGIN", spectrum.Header.Origin);
            CoreRecord("OWNER", spectrum.Header.Owner);
            OptionalRecord("SAMPLE DESCRIPTION", spectrum.Header.SampleDescription);
            OptionalRecord("DATE", spectrum.Header.MeasurementDate.ToString("yy/MM/dd"));
            OptionalRecord("TIME", spectrum.Header.MeasurementDate.ToString("HH:mm:ss"));
            OptionalRecord("$LONG DATE", spectrum.Header.MeasurementDate.ToString("yyyy/MM/dd HH:mm:ssK")); // this is not 4.24 compliant!
            OptionalRecord("SOURCE REFERENCE", spectrum.Header.SourceReference);
            OptionalRecord("CROSS REFERENCE", spectrum.Header.CrossReference);
            OptionalRecord("SPECTROMETER/DATA SYSTEM", spectrum.Header.SpectrometerSystem);
            OptionalRecord("INSTRUMENT PARAMETERS", spectrum.Header.InstrumentParameters);
            OptionalRecord("SAMPLING PROCEDURE", spectrum.Header.SamplingProcedure);
            OptionalRecord("DATA PROCESSING", spectrum.Header.DataProcessing);
            OptionalRecord("RESOLUTION", spectrum.Header.Resolution);
            CoreRecord("XUNITS", TranslateUnit(spectrum.XUnitName));
            CoreRecord("YUNITS", TranslateUnit(spectrum.YUnitName));
            CoreRecord("XFACTOR", Xfactor);
            CoreRecord("YFACTOR", Yfactor);
            CoreRecord("FIRSTX", spectrum.FirstX);
            CoreRecord("LASTX", spectrum.LastX);
            CoreRecord("NPOINTS", spectrum.Length);
            CoreRecord("FIRSTY", spectrum.FirstY);
            OptionalRecord("DELTAX", spectrum.DeltaX);
            OptionalRecord("MINX", spectrum.MinX);
            OptionalRecord("MAXX", spectrum.MaxX);
            OptionalRecord("MINY", spectrum.MinY);
            OptionalRecord("MAXY", spectrum.MaxY);
            OptionalRecord("XLABEL", spectrum.Header.XLabel);
            OptionalRecord("YLABEL", spectrum.Header.YLabel);
            OptionalRecord("CONCENTRATIONS", spectrum.Header.Concentrations);
            OptionalRecord("SAMPLINGPROCEDURE", spectrum.Header.SamplingProcedure);
            OptionalRecord("STATE", spectrum.Header.State);
            OptionalRecord("PATHLENGTH", spectrum.Header.PathLength);
            OptionalRecord("PRESSURE", spectrum.Header.Pressure);
            OptionalRecord("TEMPERATURE", spectrum.Header.Temperature);
            OptionalRecord("DATAPROCESSING", spectrum.Header.DataProcessing);
            // Hitachi U3410 specific properties, as used in MM SPC files
            OptionalRecord("$SCANMODE", spectrum.Header.ScanMode);
            OptionalRecord("$DATAMODE", spectrum.Header.DataMode);
            OptionalRecord("$SCANSPEED", spectrum.Header.ScanSpeed);
            OptionalRecord("$BASELINEMODE", spectrum.Header.BaselineMode);
            OptionalRecord("$BANDPASS_UV_VIS", spectrum.Header.BandpassUvVis);
            OptionalRecord("$BANDPASS_NIR", spectrum.Header.BandpassNir);
            OptionalRecord("$NIR_BANDPASSMODE", spectrum.Header.NirBandpassMode);
            OptionalRecord("$NIR_PBSGAIN", spectrum.Header.NirPbSGain);
            OptionalRecord("$LIGHTSOURCE", spectrum.Header.LightSource);
            OptionalRecord("$DETECTORCHANGE", spectrum.Header.DetectorChange);
            OptionalRecord("$LAMPCHANGE", spectrum.Header.LampChange);
            OptionalRecord("$RESPONSE", spectrum.Header.Response);
            OptionalRecord("$HVGAIN", spectrum.Header.HvGain);
            // SPEC Raman
            OptionalRecord("$LASERPOWER", spectrum.Header.LaserPower);
            OptionalRecord("$LASERWAVELENGTH", spectrum.Header.LaserWavelength);
            OptionalRecord("$SAMPLETIME", spectrum.Header.SampleTime);
            OptionalRecord("$SLIT1", spectrum.Header.Slit1);
            OptionalRecord("$SLIT2", spectrum.Header.Slit2);
            OptionalFreeCommentsRecord();
        }

        private void OptionalRecord(string dataLabelName, double dataNum)
        {
            if (double.IsNaN(dataNum))
                return;
            CoreRecord(dataLabelName, dataNum);
        }

        private void OptionalRecord(string dataLabelName, string dataSet)
        {
            if (string.IsNullOrEmpty(dataSet))
                return;
            CoreRecord(dataLabelName, dataSet);
        }

        private void CoreRecord(
            string dataLabelName,
            string dataSet) => stringBuilder.AppendLine(LabeledDataRecord(dataLabelName, dataSet));

        private void CoreRecord(
            string dataLabelName,
            double dataNum) => CoreRecord(dataLabelName, dataNum.ToString());

        private string LabeledDataRecord(
            string dataLabelName,
            string dataSet) => TruncateString($"{dataLabelFlag}{dataLabelName}{dataLabelTerminator}{dataSet}");

        private string TruncateString(string longString)
        {
            if (TruncateLines == false)
                return longString;
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
