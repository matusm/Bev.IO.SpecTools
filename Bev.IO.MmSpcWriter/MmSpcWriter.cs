using Bev.IO.SpectrumPod;
using System.Globalization;
using System.Text;

namespace Bev.IO.MmSpcWriter
{
    public class MmSpcWriter
    {
        private readonly Spectrum spectrum;
        private StringBuilder stringBuilder = new StringBuilder();

        public MmSpcWriter(Spectrum spectrum)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            this.spectrum = spectrum;
        }

        public string GetRecord()
        {
            ConsolidateRecords();
            stringBuilder.Clear();
            CreateKVHeader();
            CreateKVSeparator();
            CreateKVData();
            return stringBuilder.ToString();
        }

        private void CreateKVHeader() => stringBuilder.Append(spectrum.MetaDataKV);

        private void CreateKVSeparator() => stringBuilder.AppendLine("@@@@");
        
        private void CreateKVData()
        {
            foreach (var point in spectrum.Data)
            {
                stringBuilder.AppendLine(point.ToCsvString(" "));
            }
        }

        private void ConsolidateRecords()
        {
            // place for meta data cleanup
        }
    }
}
