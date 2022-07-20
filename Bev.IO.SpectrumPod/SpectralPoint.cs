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

        public override string ToString() => $"[SpectralPoint: X={X}, Y={Y}]";
    }
}
