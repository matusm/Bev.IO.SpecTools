using Bev.IO.SpectrumPod;
using System.Globalization;
using System.Text;

namespace Bev.IO.SpecCsvWriter
{
    public class CsvWriter
    {
        private readonly Spectrum spectrum;
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public CsvWriter(Spectrum spectrum)
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

        private void CreateHeader() => stringBuilder.AppendLine($"{spectrum.XUnitName},{spectrum.YUnitName}");

        private void CreateSeparator() { }

        private void CreateData()
        {
            foreach (var point in spectrum.Data)
            {
                stringBuilder.AppendLine(point.ToCsvLine());
            }
        }

        private void ConsolidateRecords()
        {
            // place for meta data cleanup
        }
    }
}
