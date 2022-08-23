namespace Bev.IO.SpectrumPod
{
    public class HeaderEntry
    {
        public string PrettyLabel = string.Empty;
        public string Value = string.Empty;
        public bool IsJcampReserved { get; }
        public bool IsRequired { get; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
        public bool IsFull => !IsEmpty;


        public HeaderEntry(string value, bool isJcampReserved, bool isRequired)
        {
            Value = value;
            IsJcampReserved = isJcampReserved;
            IsRequired = isRequired;
        }

        public string ToKVString() => $"{PrettyLabel} = {Value}";

        public string ToJcampString() => IsJcampReserved ? $"##{PrettyLabel} = {Value}" : $"##${PrettyLabel}= {Value}";
    }
}
