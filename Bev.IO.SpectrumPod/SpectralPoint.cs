using System;
using System.Globalization;

namespace Bev.IO.SpectrumPod
{
    public class SpectralPoint : IComparable<SpectralPoint>
    {
        public double X { get; }
        public double Y { get; }
        public bool IsValid => AllComponentsAreValid();

        public SpectralPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        private bool AllComponentsAreValid()
        {
            if (double.IsNaN(X))
                return false;
            if (double.IsNaN(Y))
                return false;
            return true;
        }

        public int CompareTo(SpectralPoint other) => X.CompareTo(other.X);

        public string ToCsvLine() => ToLine(",", "", "");

        public string ToLine(string separator) => ToLine(separator, ",8:F3", ",10:F6"); // X in nm, Y in %T

        public string ToLine(string separator, string xSpecifier, string ySpecifier)
        {
            string xStr = string.Format(CultureInfo.InvariantCulture, string.Format("{{0{0}}}", xSpecifier), X);
            string yStr = string.Format(CultureInfo.InvariantCulture, string.Format("{{0{0}}}", ySpecifier), Y);
            return $"{xStr}{separator}{yStr}";
        }

        public override string ToString() => $"[SpectralPoint: X={X}, Y={Y}]";
    }
}
