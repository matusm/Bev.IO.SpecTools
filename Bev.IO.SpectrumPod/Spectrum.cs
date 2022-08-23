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
        // Meta data as string in two flavours
        public string MetaDataKV => GetMetaDataAsKV(true);
        public string MetaDataJcamp => GetMetaDataAsJcamp(true);
        // User supplied meta data
        public SpectralType Type = SpectralType.Unknown;
        public DateTime? MeasurementDate;
        public DateTime? ModificationDate;
        public DateTime? SourceFileCreationDate;
        public string SourceFileName = string.Empty;
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

        public void AddMetaData(string key, string value) => header.SetMetaData(key, value);

        private string GetMetaDataAsJcamp(bool justify)
        {
            PopulateJcampComputedMetaData();
            return header.ToJcampString(justify);
        }

        private string GetMetaDataAsKV(bool justify)
        {
            PopulateJcampComputedMetaData();
            return header.ToKVString(justify);
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
            header.SetMetaData("SourceFileName", SourceFileName);
            if (MeasurementDate.HasValue)
            {
                header.SetJcampRequiredMetaData("Date", MeasurementDate.Value.ToString("yy/MM/dd"));
                header.SetJcampRequiredMetaData("Time", MeasurementDate.Value.ToString("HH:mm:ss"));
                header.SetMetaData("Long Date", MeasurementDate.Value.ToString("yyyy/MM/dd HH:mm:ssK"));
                header.SetMetaData("MeasurementDate", MeasurementDate.Value.ToString("yyyy-MM-ddTHH:mm:ssK"));
            }
            if(ModificationDate.HasValue)
                header.SetMetaData("ModificationDate", ModificationDate.Value.ToString("yyyy-MM-ddTHH:mm:ssK"));
            if (SourceFileCreationDate.HasValue)
                header.SetMetaData("SourceFileCreationDate", SourceFileCreationDate.Value.ToString("yyyy-MM-ddTHH:mm:ssK"));
            header.SetJcampRequiredMetaData("DataType", ToJcampDataType(Type));
            header.SetJcampRequiredMetaData("Length", Length.ToString());
            header.SetJcampRequiredMetaData("FirstX", FirstX.ToString());
            header.SetJcampRequiredMetaData("LastX", LastX.ToString());
            header.SetJcampRequiredMetaData("FirstY", FirstY.ToString());
            header.SetJcampMetaData("LastY", LastY.ToString());
            header.SetJcampMetaData("MaxX", MaxX.ToString());
            header.SetJcampMetaData("MinX", MinX.ToString());
            header.SetJcampMetaData("MaxY", MaxY.ToString());
            header.SetJcampMetaData("MinY", MinY.ToString());
            if(!double.IsNaN(DeltaX))
                header.SetJcampMetaData("DeltaX", DeltaX.ToString());
            header.SetJcampRequiredMetaData("XUnits", XUnitName);
            header.SetJcampRequiredMetaData("YUnits", YUnitName);
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


        private const double epsilon = 0.000001; // TODO: works for Perkin Elmer spectrophotometer ascii files
        private readonly MetaData header = new MetaData();
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
