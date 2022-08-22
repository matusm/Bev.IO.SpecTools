using System;
using System.Collections.Generic;

namespace Bev.IO.SpectrumPod
{
    public class SpectralHeader
    {
        public Dictionary<string, HeaderEntry> Parameters = new Dictionary<string, HeaderEntry>();

        // special properties
        public SpectralType Type = SpectralType.Unknown;
        public DateTime MeasurementDate;
        public DateTime ModificationDate;
        public DateTime OriginalFileCreationDate;
        public string OriginalFileName = string.Empty;

        public SpectralHeader()
        {
            PopulateJcampRequiredProperties();
        }

        public string SpectrometerModel = string.Empty;
        public string SpectrometerSerialNumber = string.Empty;
        public string SoftwareID = string.Empty;
        public string[] FreeComments = new string[0];       // avoid returning null 
        // JCAMP-DX 4.24 specific properties
        public string Title = string.Empty;                 // JCAMP-DX required! original filename? sample description
        public string DataType => ToJcampDataType(Type);    // JCAMP-DX required! INFRARED SPECTRUM, UV/VIS SPECTRUM, RAMAN SPECTRUM , ...
        public string Origin = string.Empty;                // JCAMP-DX required! ??? Exported PE Spectrum Data File, BEV
        public string Owner = string.Empty;                 // JCAMP-DX required! person who made the measurement 
        public string SpectrometerSystem = string.Empty;    // JCAMP-DX optional! model + serial number
        public string InstrumentParameters = string.Empty;  // JCAMP-DX optional! many - how to select?
        public string SampleDescription = string.Empty;     // JCAMP-DX optional! important
        public string Concentrations = string.Empty;        // JCAMP-DX optional!
        public string SamplingProcedure = string.Empty;     // JCAMP-DX optional!
        public string State = string.Empty;                 // JCAMP-DX optional! eg glass filter
        public string PathLength = string.Empty;            // JCAMP-DX optional!
        public string Pressure = string.Empty;              // JCAMP-DX optional!
        public string Temperature = string.Empty;           // JCAMP-DX optional! -> filter temperature?
        public string DataProcessing = string.Empty;        // JCAMP-DX optional. -> none or from software
        public string SourceReference = string.Empty;       // JCAMP-DX optional. -> original filename !
        public string CrossReference = string.Empty;        // JCAMP-DX optional.
        public string Resolution = string.Empty;            // JCAMP-DX optional. // also for Raman SPC
        public string XLabel = string.Empty;                // JCAMP-DX optional.
        public string YLabel = string.Empty;                // JCAMP-DX optional.
        // PerkinElmer Lambda 1050+ specific properties
        public string Lamps;
        public string Accessories;
        public string UvVisSlitMode;
        public string NirSlitMode;
        public string NirSlitWidth;
        public string UvVisIntegrationTime;
        public string NirIntegrationTime;
        public string NirGain;
        public string MonochromatorChange;
        public string SampleBeamPosition;
        public string CommonBeamMask;
        public string CommonBeamDepolarizer;
        public string Attenuators;
        // Hitachi U3410 specific properties, as used in MM SPC files
        public string ScanMode;                     // scan/Time/Rep
        public string DataMode;                     // %T/Abs/SB
        public double ScanSpeed = double.NaN;       // nm/min
        public string BaselineMode;                 // user/sys
        public double BandpassUvVis = double.NaN;   // in nm
        public double BandpassNir = double.NaN;     // in nm
        public string NirBandpassMode;              // fix/servo
        public double NirPbSGain = double.NaN;      // 1
        public string LightSource;                  // Auto/W/D2
        public string DetectorChange;               // 840 nm
        public double LampChange = double.NaN;      // 340 nm
        public string Response;                     // fast/medium/slow
        public double HvGain = double.NaN;          // 200
        // Raman, SPEX 11418
        public double LaserPower = double.NaN;      // in mW
        public double LaserWavelength = double.NaN; // in nm
        public double SampleTime = double.NaN;      // in s
        public double Slit1 = double.NaN;           // in µm
        public double Slit2 = double.NaN;           // in µm

        // SPC:***Raman
        // Spex 11418 Doppelmonochromator, f = 85 cm, 1800 mm-1
        // PMT C31034A + Photoncounter SSR Instruments 1110
        //
        // Dilor XY Dreifachmonochromator, N2 cooled CCD
        // f = 60 cm, 1800 mm-1

        private void PopulateJcampRequiredProperties()
        {
            SetRequiredParameter("Title", string.Empty);                 // JCAMP-DX required! original filename? sample description
            SetRequiredParameter("JCAMP-DX", "4.24");
            SetRequiredParameter("DataType", string.Empty);              // TODO JCAMP-DX required! INFRARED SPECTRUM, UV/VIS SPECTRUM, RAMAN SPECTRUM , ...
            SetRequiredParameter("Origin", string.Empty);                // JCAMP-DX required! ??? Exported PE Spectrum Data File, BEV
            SetRequiredParameter("Owner", string.Empty);                 // JCAMP-DX required! person who made the measurement 
            SetOptionalParameter("SpectrometerSystem", string.Empty);    // JCAMP-DX optional! model + serial number
            SetOptionalParameter("InstrumentParameters", string.Empty);  // JCAMP-DX optional! many - how to select?
            SetOptionalParameter("SampleDescription", string.Empty);     // JCAMP-DX optional! important
            SetOptionalParameter("Concentrations", string.Empty);        // JCAMP-DX optional!
            SetOptionalParameter("SamplingProcedure", string.Empty);     // JCAMP-DX optional!
            SetOptionalParameter("State", string.Empty);                 // JCAMP-DX optional! eg glass filter
            SetOptionalParameter("PathLength", string.Empty);            // JCAMP-DX optional!
            SetOptionalParameter("Pressure", string.Empty);              // JCAMP-DX optional!
            SetOptionalParameter("Temperature", string.Empty);           // JCAMP-DX optional! -> filter temperature?
            SetOptionalParameter("DataProcessing", string.Empty);        // JCAMP-DX optional. -> none or from software
            SetOptionalParameter("SourceReference", string.Empty);       // JCAMP-DX optional. -> original filename !
            SetOptionalParameter("CrossReference", string.Empty);        // JCAMP-DX optional.
            SetOptionalParameter("Resolution", string.Empty);            // JCAMP-DX optional. // also for Raman SPC
            SetOptionalParameter("XLabel", string.Empty);                // JCAMP-DX optional.
            SetOptionalParameter("YLabel", string.Empty);                // JCAMP-DX optional.
        }

        public void SetOptionalParameter(string key, string value)
        {
            Parameters[key.Trim()] = new HeaderParameter(value);
        }

        public void SetRequiredParameter(string key, string value)
        {
            Parameters[key.Trim()] = new HeaderParameter(value, true);
        }

        public string GetParameter(string key)
        {
            // TODO implement special cases
            try
            {
                return Parameters[key].Value; // does not work with beautified keys!
            }
            catch (KeyNotFoundException)
            {
                return string.Empty;
            }
        }

        public void PopulateComputedProperties()
        {
            SetRequiredParameter("DataType", ToJcampDataType(Type));
            SetRequiredParameter("Length", 0.ToString());

        }

        public void BeautifyKeys(bool toUpper)
        {
            int maxKeyLength = GetMaximumKeyLength();
            foreach (var k in Parameters.Keys)
            {
                string bKey = GetBeautifiedKey(k, maxKeyLength, toUpper);
                Parameters[k].PrettyLabel = bKey;
            }
        }


        private string GetBeautifiedKey(string key, int maximumKeyLength, bool toUpper)
        {
            string beautyString = key.PadRight(maximumKeyLength);
            if (toUpper) beautyString.ToUpperInvariant();
            return beautyString;
        }

        private int GetMaximumKeyLength() 
        {
            // determine the length of the longest (trimmed) key
            int maxKeyLength = 0;
            foreach (string k in Parameters.Keys)
                if (k.Length > maxKeyLength) maxKeyLength = k.Length;
            return maxKeyLength;
        }

        private string ToJcampDataType(SpectralType type)
        {
            switch (type)
            {
                case SpectralType.Unknown:
                    return string.Empty;
                case SpectralType.Raman:
                    return "RAMAN SPECTRUM";
                case SpectralType.Infrared:
                    return "INFRARED SPECTRUM";
                case SpectralType.UvVis:
                    return "UV/VIS SPECTRUM";
                case SpectralType.Nmr:
                    return "NMR SPECTRUM";
                case SpectralType.Mass:
                    return "MASS SPECTRUM";
                default:
                    return string.Empty;
            }
        }

    }


}
