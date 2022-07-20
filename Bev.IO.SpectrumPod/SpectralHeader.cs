using System;

namespace Bev.IO.SpectrumPod
{
    public class SpectralHeader
    {
        public DateTime MeasurementDate;
        public DateTime ModificationDate;
        public string Title = string.Empty;                 // original filename?
        public string DataType = string.Empty;              // INFRARED SPECTRUM, UV/VIS SPECTRUM, RAMAN SPECTRUM , ...
        public string Origin = string.Empty;                // ??? Exported PE Spectrum Data File, BEV
        public string Owner = string.Empty;                 // person who made the measurement 
        public string SpectrometerSystem = string.Empty;    // for jcamp-dx compatibility, model + serial number
        public string InstrumentParameters = string.Empty;  // for jcamp-dx compatibility, many - how to select?
        public string SampleDescription = string.Empty;     // important
        public string Concentrations = string.Empty;        // for jcamp-dx compatibility
        public string SamplingProcedure = string.Empty;     // for jcamp-dx compatibility
        public string State = string.Empty;                 // for jcamp-dx compatibility, glass filter
        public string PathLength = string.Empty;            // for jcamp-dx compatibility
        public string Pressure = string.Empty;              // for jcamp-dx compatibility
        public string Temperature = string.Empty;           // for jcamp-dx compatibility -> filter temperature?
        public string DataProcessing = string.Empty;        // for jcamp-dx compatibility -> none or from software
        public string SourceReference = string.Empty;       // for jcamp-dx compatibility -> original filename !
        public string CrossReference = string.Empty;        // for jcamp-dx compatibility
        public string Class = string.Empty;                 // for jcamp-dx compatibility, now useless
        public string Resolution = string.Empty;            // bandwidth in nm ?
        public string SpectrometerModel = string.Empty;
        public string SpectrometerSerialNumber = string.Empty;
        public string SoftwareID = string.Empty;
    }
}
