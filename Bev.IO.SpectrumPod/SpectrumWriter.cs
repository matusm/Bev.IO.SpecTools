using Bev.IO.SpectrumPod;
using System.Globalization;
using System.Text;

namespace Bev.IO.SpectrumPod
{
    public abstract class SpectrumWriter
    {
        private readonly Spectrum spectrum;
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public SpectrumWriter(Spectrum spectrum)
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

        public abstract void CreateHeader();

        public abstract void CreateSeparator();

        public abstract void CreateData();

        public abstract void ConsolidateRecords();


    }
}
