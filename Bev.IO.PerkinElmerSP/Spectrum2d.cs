using CsvHelper;

namespace Bev.IO.PerkinElmerSP
{
    public class Spectrum2d : IData
    {
        public double StartX { get; set; }
        public double EndX { get; set; }
        public double ResolutionX { get; set; }
        public string LabelX { get; set; }
        public string LabelY { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public double[] PointsY { get; set; }

        public void WriteCsv(CsvWriter w)
        {
            //Header
            w.WriteField(LabelX);
            w.WriteField(LabelY);
            w.NextRecord();
            //Rows
            double x = StartX;
            foreach (var item in PointsY)
            {
                w.WriteField(x);
                w.WriteField(item);
                w.NextRecord();
                x += ResolutionX;
            }
        }

        public override string ToString()
        {
            return $"[Spectrum2d StartX={StartX} EndX={EndX} ResolutionX={ResolutionX} LabelX={LabelX} LabelY={LabelY} Name={Name} Alias={Alias} NPointsY={PointsY.Length}]";
        }

    }
}
