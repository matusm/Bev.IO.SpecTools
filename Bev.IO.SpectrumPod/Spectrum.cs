using At.Matus.StatisticPod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bev.IO.SpectrumPod
{
    public class Spectrum
    {
        // Actual spectrum 
        public SpectralPoint[] Data => spectralData.ToArray();
        // Meta data for spectrum
        private MetaData Header = new MetaData();
        // User supplied meta data
        public SpectralType Type = SpectralType.Unknown;
        public DateTime? MeasurementDate;
        public DateTime? ModificationDate;
        public DateTime? OriginalFileCreationDate;
        public string OriginalFileName = string.Empty;
        public string XUnitName { get; private set; } = string.Empty;
        public string YUnitName { get; private set; } = string.Empty;
        // Computed meta data properties 
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

        public Spectrum() : this(SortOrder.Ascending){}

        public Spectrum(SortOrder sortOrder)
        {
            this.sortOrder = sortOrder;
        }

        public void SetUnitNames(string forX, string forY)
        {
            XUnitName = forX.Trim();
            YUnitName = forY.Trim();
        }

        public void AddDataValue(SpectralPoint value)
        {
            spectralData.Add(value);
            xStat.Update(value.X);
            yStat.Update(value.Y);
            SortData(sortOrder);
        }

        public void AddDataValue(double xValue, double yValue) => AddDataValue(new SpectralPoint(xValue, yValue));

        public void DeleteAllDataValues()
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

        private void PopulateJcampComputedMetaData()
        {
            Header.SetMetaData("OriginalFileName", OriginalFileName);
            if (MeasurementDate.HasValue)
            {
                Header.SetJcampRequiredMetaData("Date", MeasurementDate.Value.ToString("yy/MM/dd"));
                Header.SetJcampRequiredMetaData("Time", MeasurementDate.Value.ToString("HH:mm:ss"));
                Header.SetMetaData("Long Date", MeasurementDate.Value.ToString("yyyy/MM/dd HH:mm:ssK"));
                Header.SetMetaData("MeasurementDate", MeasurementDate.Value.ToString("yyyy-MM-ddTHH:mm:ssK"));
            }
            if(ModificationDate.HasValue)
                Header.SetMetaData("ModificationDate", ModificationDate.Value.ToString("yyyy-MM-ddTHH:mm:ssK"));
            if (OriginalFileCreationDate.HasValue)
                Header.SetMetaData("OriginalFileCreationDate", OriginalFileCreationDate.Value.ToString("yyyy-MM-ddTHH:mm:ssK"));
            Header.SetJcampRequiredMetaData("DataType", ToJcampDataType(Type));
            Header.SetJcampRequiredMetaData("Length", Length.ToString());
            Header.SetJcampRequiredMetaData("FirstX", FirstX.ToString());
            Header.SetJcampRequiredMetaData("LastX", LastX.ToString());
            Header.SetJcampRequiredMetaData("FirstY", FirstY.ToString());
            Header.SetJcampMetaData("LastY", LastY.ToString());
            Header.SetJcampMetaData("MaxX", MaxX.ToString());
            Header.SetJcampMetaData("MinX", MinX.ToString());
            Header.SetJcampMetaData("MaxY", MaxY.ToString());
            Header.SetJcampMetaData("MinY", MinY.ToString());
            if(!double.IsNaN(DeltaX))
                Header.SetJcampMetaData("DeltaX", DeltaX.ToString());
            Header.SetJcampRequiredMetaData("XUnits", XUnitName);
            Header.SetJcampRequiredMetaData("YUnits", YUnitName);
        }

        private string ToJcampDataType(SpectralType type)
        {
            switch (type)
            {
                case SpectralType.Unknown:
                    return string.Empty;
                case SpectralType.Raman:
                    return "RAMAN SPECTRUM";
                case SpectralType.Infrared:
                    return "INFRARED SPECTRUM";
                case SpectralType.UvVis:
                    return "UV/VIS SPECTRUM";
                case SpectralType.Nmr:
                    return "NMR SPECTRUM";
                case SpectralType.Mass:
                    return "MASS SPECTRUM";
                default:
                    return string.Empty;
            }
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

    public enum SpectralType
    {
        Unknown,
        Raman,
        Infrared,
        UvVis,
        Nmr,
        Mass
    }
}
