namespace Bev.IO.SpectrumPod
{
    public class HeaderRecord
    {
        //public string PrettyLabel = string.Empty;
        public string PlainLabel = string.Empty;
        public string Value = string.Empty;
        public bool IsJcampReserved { get; }
        public bool IsRequired { get; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
        public bool IsFull => !IsEmpty;

        public HeaderRecord(string value, bool isJcampReserved, bool isRequired)
        {
            Value = value;
            IsJcampReserved = isJcampReserved;
            IsRequired = isRequired;
        }

        public string ToKVString(int totalWidth) => ToHeaderString(false, false, totalWidth);

        public string ToJcampString(int totalWidth) => ToHeaderString(true, true, totalWidth);

        private string ToHeaderString(bool isJcamp, bool toUpper, int beautify)
        {
            string prettyLabel = PlainLabel;
            if (toUpper)
                prettyLabel = prettyLabel.ToUpperInvariant();
            if (isJcamp)
            {
                prettyLabel = IsJcampReserved ? $"##{prettyLabel}=" : $"##${prettyLabel}=";
                prettyLabel = JustifiedLabel(prettyLabel, beautify + 4);
                return $"{prettyLabel} {Value}";
            }
            prettyLabel = JustifiedLabel(prettyLabel, beautify);
            return $"{prettyLabel} = {Value}";
        }

        private string JustifiedLabel (string label, int totalWidth)
        {
            if (totalWidth > 4)
                return label.PadRight(totalWidth);
            return label;
        }

        private string TruncateString(string longString, int maxColumns)
        {
            if (maxColumns <= 0)
                return longString;
            if (string.IsNullOrEmpty(longString))
                return longString;
            if (longString.Length <= maxColumns)
                return longString;
            return $"{longString.Substring(0, maxColumns - 3)}...";
        }
    }
}
