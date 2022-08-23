using Bev.IO.SpectrumPod;
using System;
using System.Globalization;
using System.Text;

namespace Bev.IO.JcampDxWriter
{
    public class JcampWriter
    {
        private const string tabularIndend = "";
        private const int maxColumns = 80;
        private readonly Spectrum spectrum;
        private StringBuilder stringBuilder = new StringBuilder();

        public double Xfactor = 1;
        public double Yfactor = 1;
        public bool TruncateLines = false;

        public JcampWriter(Spectrum spectrum)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            this.spectrum = spectrum;
        }

        public string GetRecord()
        {
            ConsolidateRecords();
            stringBuilder.Clear();
            CreateJcampHeader();
            CreateJcampData();
            return stringBuilder.ToString();
        }

        private void ConsolidateRecords()
        {
            spectrum.AddMetaData("XUnits", TranslateUnit(spectrum.XUnitName));
            spectrum.AddMetaData("YUnits", TranslateUnit(spectrum.YUnitName));
            spectrum.AddMetaData("XFactor", Xfactor.ToString());
            spectrum.AddMetaData("YFactor", Yfactor.ToString());
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
            stringBuilder.AppendLine(spectrum.MetaDataJcamp);

            //// file properties
            //OptionalRecord("$FILENAME", spectrum.Header.OriginalFileName);
            //OptionalRecord("$FILECREATIONDATE", spectrum.Header.OriginalFileCreationDate.ToString("yyyy/MM/dd HH:mm:ssK"));
            //// Hitachi U3410 specific properties, as used in MM SPC files
            //OptionalRecord("$SCANMODE", spectrum.Header.ScanMode);
            //OptionalRecord("$DATAMODE", spectrum.Header.DataMode);
            //OptionalRecord("$SCANSPEED", spectrum.Header.ScanSpeed);
            //OptionalRecord("$BASELINEMODE", spectrum.Header.BaselineMode);
            //OptionalRecord("$BANDPASS_UV_VIS", spectrum.Header.BandpassUvVis);
            //OptionalRecord("$BANDPASS_NIR", spectrum.Header.BandpassNir);
            //OptionalRecord("$NIR_BANDPASSMODE", spectrum.Header.NirBandpassMode);
            //OptionalRecord("$NIR_PBSGAIN", spectrum.Header.NirPbSGain);
            //OptionalRecord("$LIGHTSOURCE", spectrum.Header.LightSource);
            //OptionalRecord("$DETECTORCHANGE", spectrum.Header.DetectorChange);
            //OptionalRecord("$LAMPCHANGE", spectrum.Header.LampChange);
            //OptionalRecord("$RESPONSE", spectrum.Header.Response);
            //OptionalRecord("$HVGAIN", spectrum.Header.HvGain);
            //// SPEC Raman
            //OptionalRecord("$LASERPOWER", spectrum.Header.LaserPower);
            //OptionalRecord("$LASERWAVELENGTH", spectrum.Header.LaserWavelength);
            //OptionalRecord("$SAMPLETIME", spectrum.Header.SampleTime);
            //OptionalRecord("$SLIT1", spectrum.Header.Slit1);
            //OptionalRecord("$SLIT2", spectrum.Header.Slit2);
        }

        private void CoreRecord(string dataLabelName, string dataSet)
        {
            HeaderEntry he = new HeaderEntry(dataSet, true, true);
            he.PrettyLabel = dataLabelName;
            stringBuilder.AppendLine(he.ToJcampString());
        }

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
