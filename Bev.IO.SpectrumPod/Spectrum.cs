﻿using At.Matus.StatisticPod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bev.IO.SpectrumPod
{
    public class Spectrum
    {
        public SpectralHeader Header;
        public SpectralPoint[] Data => spectralData.ToArray();

        public SpectralSpacing AbscissaType => EstimateSpacingType();
        public int Length => spectralData.Count;
        public double FirstX => spectralData.First().X;
        public double LastX => spectralData.Last().X;
        public double FirstY => spectralData.First().Y;
        public double LastY => spectralData.Last().Y;
        public double MaxX => xStat.MaximumValue;
        public double MinX => xStat.MinimumValue;
        public double MaxY => yStat.MaximumValue;
        public double MinY => yStat.MinimumValue;
        public double DeltaX => CalculateDeltaX();
        public string XUnitName { get; private set; } = string.Empty;
        public string YUnitName { get; private set; } = string.Empty;

        public Spectrum() : this(SortOrder.Ascending){}

        public Spectrum(SortOrder sortOrder)
        {
            this.sortOrder = sortOrder;
            Header = new SpectralHeader();
        }

        public void SetUnitNames(string forX, string forY)
        {
            XUnitName = forX.Trim();
            YUnitName = forY.Trim();
        }

        public void AddValue(SpectralPoint value)
        {
            spectralData.Add(value);
            xStat.Update(value.X);
            yStat.Update(value.Y);
            SortData(sortOrder);
        }

        public void AddValue(double xValue, double yValue) => AddValue(new SpectralPoint(xValue, yValue));

        public void DeleteAllValues()
        {
            spectralData.Clear();
            xStat.Restart();
            yStat.Restart();
        }

        private SpectralSpacing EstimateSpacingType()
        {
            SpectralPoint[] spec = Data;
            if (spec.Length < 3)
                return SpectralSpacing.Unknown;
            StatisticPod spacingStatistics = new StatisticPod();
            for (int i = 0; i < spec.Length - 1; i++)
            {
                spacingStatistics.Update(spec[i + 1].X - spec[i].X);
            }
            double rangeOfSpacings = Math.Abs(spacingStatistics.Range);
            if (rangeOfSpacings < epsilon)
                return SpectralSpacing.FixedSpacing;
            return SpectralSpacing.VariableSpacing;
        }

        private double CalculateDeltaX()
        {
            if (AbscissaType == SpectralSpacing.FixedSpacing)
                return (LastX - FirstX) / (Length - 1);
            return double.NaN;
        }

        private void SortData(SortOrder sortOrder)
        {
            if (sortOrder == SortOrder.None)
                return;
            spectralData.Sort();
            if (sortOrder == SortOrder.Ascending)
                return;
            spectralData.Reverse();
        }

        private const double epsilon = 0.000001; // TODO works for Perkin Elmer spectrophotometer ascii files
        private readonly List<SpectralPoint> spectralData = new List<SpectralPoint>();
        private readonly SortOrder sortOrder;
        private readonly StatisticPod xStat = new StatisticPod();
        private readonly StatisticPod yStat = new StatisticPod();
    }

    public enum SpectralSpacing
    {
        Unknown,
        FixedSpacing,   // ##XYDATA= (X++(Y..Y))
        VariableSpacing // ##XYPOINTS= (XY..XY)
    }

    public enum SortOrder
    {
        None,
        Ascending,
        Descending
    }
}
