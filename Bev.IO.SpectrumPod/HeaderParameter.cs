namespace Bev.IO.SpectrumPod
{
    public class HeaderParameter
    {
        public string PrettyKey = string.Empty;
        public string Value { get; }
        public bool IsRequired { get; }

        public HeaderParameter(string value, bool isRequired)
        {
            Value = value;
            IsRequired = isRequired;
        }

        public HeaderParameter(string value) : this(value, false) { }
    }
}
