using System.Collections.Generic;
using System.Text;

namespace Bev.IO.SpectrumPod
{
    public class MetaData
    {
        private readonly Dictionary<string, HeaderRecord> metaDataDictonary = new Dictionary<string, HeaderRecord>();

        public MetaData()
        {
            PopulateJcampMetaData();
        }

        public void SetMetaData(string key, string value) => SetMetaData(key, value, false, false);

        public void SetMetaData(string key) => SetMetaData(key, string.Empty);

        public void SetJcampMetaData(string key, string value) => SetMetaData(key, value, true, false);

        public void SetJcampMetaData(string key) => SetJcampMetaData(key, string.Empty);

        public void SetJcampRequiredMetaData(string key, string value) => SetMetaData(key, value, true, true);

        public void SetJcampRequiredMetaData(string key) => SetJcampRequiredMetaData(key, string.Empty);

        private void SetMetaData(string key, string value, bool isJcamp, bool isRequired)
        {
            string trimmedKey = key.Trim();
            if (metaDataDictonary.ContainsKey(trimmedKey))
            {
                metaDataDictonary[trimmedKey].Value = value;
                return;
            }
            metaDataDictonary[trimmedKey] = new HeaderRecord(value, isJcamp, isRequired);
            metaDataDictonary[trimmedKey].PlainLabel = trimmedKey;
        }

        public string ToKVString(bool justify)
        {
            int totalLabelWidth = -1; // do not justify GetMaximumKeyLength();
            if (justify)
                totalLabelWidth = GetMaximumKeyLength();
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, HeaderRecord> entry in metaDataDictonary)
            {
                if(entry.Value.IsFull && !EntryIsObsoleteForKV(entry.Key))
                    sb.AppendLine(entry.Value.ToKVString(totalLabelWidth));
            }
            return sb.ToString();
        }

        public string ToJcampString(bool justify)
        {
            int totalLabelWidth = -1; // do not justify GetMaximumKeyLength();
            if (justify)
                totalLabelWidth = GetMaximumKeyLength();
            StringBuilder sb = new StringBuilder();
            foreach (var entry in metaDataDictonary)
            {
                if (entry.Value.IsFull || entry.Value.IsRequired)
                    sb.AppendLine(entry.Value.ToJcampString(totalLabelWidth));
            }
            return sb.ToString();
        }

        private void PopulateJcampMetaData()
        {
            SetJcampRequiredMetaData("Title");                  // JCAMP-DX required! original filename? sample description
            SetJcampRequiredMetaData("JCAMP-DX", "4.24");
            SetJcampRequiredMetaData("DataType");               // TODO JCAMP-DX required! INFRARED SPECTRUM, UV/VIS SPECTRUM, RAMAN SPECTRUM , ...
            SetJcampRequiredMetaData("Origin");                 // JCAMP-DX required! ??? Exported PE Spectrum Data File, BEV
            SetJcampRequiredMetaData("Owner");                  // JCAMP-DX required! person who made the measurement 
            SetJcampMetaData("SpectrometerSystem");             // JCAMP-DX optional! model + serial number
            SetJcampMetaData("InstrumentParameters");           // JCAMP-DX optional! many - how to select?
            SetJcampMetaData("SampleDescription");              // JCAMP-DX optional! important
            SetJcampMetaData("Concentrations");                 // JCAMP-DX optional!
            SetJcampMetaData("SamplingProcedure");              // JCAMP-DX optional!
            SetJcampMetaData("State");                          // JCAMP-DX optional! eg glass filter
            SetJcampMetaData("PathLength");                     // JCAMP-DX optional!
            SetJcampMetaData("Pressure");                       // JCAMP-DX optional!
            SetJcampMetaData("Temperature");                    // JCAMP-DX optional! -> filter temperature?
            SetJcampMetaData("DataProcessing");                 // JCAMP-DX optional. -> none or from software
            SetJcampMetaData("SourceReference");                // JCAMP-DX optional. -> original filename !
            SetJcampMetaData("CrossReference");                 // JCAMP-DX optional.
            SetJcampMetaData("Resolution");                     // JCAMP-DX optional. // also for Raman SPC
            SetJcampMetaData("XLabel");                         // JCAMP-DX optional.
            SetJcampMetaData("YLabel");                         // JCAMP-DX optional.
            SetJcampRequiredMetaData("XFactor");                // JCAMP-DX required!
            SetJcampRequiredMetaData("YFactor");                // JCAMP-DX required!
        }

        private int GetMaximumKeyLength()
        {
            // determine the length of the longest (trimmed) key
            int maxKeyLength = 0;
            foreach (string k in metaDataDictonary.Keys)
                if (k.Length > maxKeyLength) maxKeyLength = k.Length;
            return maxKeyLength;
        }

        private readonly string[] obsoleteKeys = { "JCAMP-DX", "Date", "Time", "Long Date", "SpectrometerSystem" };

        private bool EntryIsObsoleteForKV(string key)
        {
            foreach (var s in obsoleteKeys)
                if (key == s) return true;
            return false;
        }

    }
}
