using Bev.IO.SpectrumPod;
using System.Globalization;
using System.Text;

namespace Bev.IO.MmSpcWriter
{
    public class MmSpcWriter
    {
        private readonly Spectrum spectrum;
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public MmSpcWriter(Spectrum spectrum)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            this.spectrum = spectrum;
        }

        public string GetRecord()
        {
            ConsolidateRecords();
            stringBuilder.Clear();
            CreateHeader();
            CreateSeparator();
            CreateData();
            return stringBuilder.ToString();
        }

        private void CreateHeader() => stringBuilder.Append(spectrum.MetaDataKV);

        private void CreateSeparator() => stringBuilder.AppendLine("@@@@");
        
        private void CreateData()
        {
            foreach (var point in spectrum.Data)
            {
                stringBuilder.AppendLine(point.ToLine(" "));
            }
        }

        private void ConsolidateRecords()
        {
            // place for meta data cleanup
        }
    }
}
