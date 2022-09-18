using CsvHelper;

namespace Bev.IO.PerkinElmerSP
{
    public interface IData
    {
        public void WriteCsv(CsvWriter w);
    }
}
