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
        public Dictionary<string, HeaderParameter> MetaData = new Dictionary<string, HeaderParameter>();
        // User supplied meta data
        public SpectralType Type = SpectralType.Unknown;
        public DateTime MeasurementDate;
        public DateTime ModificationDate;
        public DateTime OriginalFileCreationDate;
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
            PopulateJcampMetaData();
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

        private void PopulateJcampMetaData()
        {
            SetRequiredMetaData("Title");                 // JCAMP-DX required! original filename? sample description
            SetRequiredMetaData("JCAMP-DX", "4.24");
            SetRequiredMetaData("DataType");              // TODO JCAMP-DX required! INFRARED SPECTRUM, UV/VIS SPECTRUM, RAMAN SPECTRUM , ...
            SetRequiredMetaData("Origin");                // JCAMP-DX required! ??? Exported PE Spectrum Data File, BEV
            SetRequiredMetaData("Owner");                 // JCAMP-DX required! person who made the measurement 
            SetOptionalMetaData("SpectrometerSystem");    // JCAMP-DX optional! model + serial number
            SetOptionalMetaData("InstrumentParameters");  // JCAMP-DX optional! many - how to select?
            SetOptionalMetaData("SampleDescription");     // JCAMP-DX optional! important
            SetOptionalMetaData("Concentrations");        // JCAMP-DX optional!
            SetOptionalMetaData("SamplingProcedure");     // JCAMP-DX optional!
            SetOptionalMetaData("State");                 // JCAMP-DX optional! eg glass filter
            SetOptionalMetaData("PathLength");            // JCAMP-DX optional!
            SetOptionalMetaData("Pressure");              // JCAMP-DX optional!
            SetOptionalMetaData("Temperature");           // JCAMP-DX optional! -> filter temperature?
            SetOptionalMetaData("DataProcessing");        // JCAMP-DX optional. -> none or from software
            SetOptionalMetaData("SourceReference");       // JCAMP-DX optional. -> original filename !
            SetOptionalMetaData("CrossReference");        // JCAMP-DX optional.
            SetOptionalMetaData("Resolution");            // JCAMP-DX optional. // also for Raman SPC
            SetOptionalMetaData("XLabel");                // JCAMP-DX optional.
            SetOptionalMetaData("YLabel");                // JCAMP-DX optional.
        }

        private void PopulateJcampComputedMetaData()
        {
            // TODO dates
            SetRequiredMetaData("DataType", ToJcampDataType(Type));
            SetRequiredMetaData("Length", Length.ToString());
            SetRequiredMetaData("FirstX", FirstX.ToString());
            SetRequiredMetaData("LastX", LastX.ToString());
            SetRequiredMetaData("FirstY", FirstY.ToString());
            SetRequiredMetaData("LastY", LastY.ToString());
            SetRequiredMetaData("MaxX", MaxX.ToString());
            SetRequiredMetaData("MinX", MinX.ToString());
            SetRequiredMetaData("MaxY", MaxY.ToString());
            SetRequiredMetaData("MinY", MinY.ToString());
            if(!double.IsNaN(DeltaX)) 
                SetOptionalMetaData("DeltaX", DeltaX.ToString());
            SetRequiredMetaData("XUnit", XUnitName);
            SetRequiredMetaData("YUnit", YUnitName);
        }

        public void SetOptionalMetaData(string key, string value)
        {
            string trimmedKey = key.Trim();
            MetaData[trimmedKey] = new HeaderParameter(value, false);
            MetaData[trimmedKey].PrettyKey = trimmedKey;
        }

        public void SetOptionalMetaData(string key) => SetOptionalMetaData(key, string.Empty);

        public void SetRequiredMetaData(string key, string value)
        {
            string trimmedKey = key.Trim();
            MetaData[trimmedKey] = new HeaderParameter(value, true);
            MetaData[trimmedKey].PrettyKey = trimmedKey;
        }

        public void SetRequiredMetaData(string key) => SetRequiredMetaData(key, string.Empty);

        public void BeautifyKeys(bool toUpper)
        {
            int maxKeyLength = GetMaximumKeyLength();
            foreach (var k in MetaData.Keys)
            {
                string bKey = GetBeautifiedKey(k, maxKeyLength, toUpper);
                MetaData[k].PrettyKey = bKey;
            }
        }

        private string GetBeautifiedKey(string key, int maximumKeyLength, bool toUpper)
        {
            string beautyString = key.PadRight(maximumKeyLength);
            if (toUpper) beautyString.ToUpperInvariant();
            return beautyString;
        }

        private int GetMaximumKeyLength()
        {
            // determine the length of the longest (trimmed) key
            int maxKeyLength = 0;
            foreach (string k in MetaData.Keys)
                if (k.Length > maxKeyLength) maxKeyLength = k.Length;
            return maxKeyLength;
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
}
