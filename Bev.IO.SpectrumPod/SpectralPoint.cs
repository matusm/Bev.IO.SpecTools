using System;

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

        public string ToCsvString(string separator)
        {
            return $"{X,11:F6}{separator}{Y,11:F6}";
        }

        public override string ToString() => $"[SpectralPoint: X={X}, Y={Y}]";
    }
}
