using Bev.IO.SpectrumPod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Bev.IO.PerkinElmerSP
{
    public class HdrHistoryParser
    {
        private HdrType hdrType = HdrType.Unkown;

        public string[] HdrHistory { get; private set; }

        public HdrHistoryParser(TypedBlock historyBlock)
        {
            ParseBlock(historyBlock);
        }

        public void AddAsMetaData(Spectrum spectrum)
        {
            spectrum.AddMetaData("Owner", HdrHistory[0]);
            spectrum.AddMetaData("SampleDescription", HdrHistory[4]);
            spectrum.AddMetaData("Title", HdrHistory[4]);
        }

        public string ToDebugString()
        {
            if (HdrHistory == null) return "no HDR history";
            if (HdrHistory.Length < 1) return "no HDR history";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < HdrHistory.Length; i++)
                sb.AppendLine($"{i,2} : >{HdrHistory[i]}<");
            return sb.ToString();
        }

        private void ParseBlock (TypedBlock tb)
        {
            if ((BlockCodes)tb.Id != BlockCodes.DataSetHistoryRecord)
                return; // only parse DataSetHistoryRecord blocks
            if ((BlockCodes)tb.TypeCode == BlockCodes.InstrHdrHistoryRecord)
            {
                hdrType = HdrType.Simple;
                HdrHistory = SegmentHdrHistory(tb.Data);
            }
            if ((BlockCodes)tb.TypeCode == BlockCodes.HistoryRecord)
            {
                //TODO this is actually a block of few blocks!
                hdrType = HdrType.Compound;
                HdrHistory = SegmentHdrHistory(tb.Data);
            }
        }

        private string[] SegmentHdrHistory(byte[] data)
        {
            List<string> hdrLines = new List<string>();
            if (data.Length < 5)
            {
                hdrLines.Add("no HDR history!");
                return hdrLines.ToArray();
            }
            for (int i = 1; i < data.Length - 1; i++)
            {
                if (data[i - 1] == 0x23 && data[i] == 0x75)
                {
                    int len = BitConverter.ToInt16(data, i + 1);
                    string line = Encoding.ASCII.GetString(data, i + 3, len);
                    hdrLines.Add(RemoveLineEndings(line));
                }
            }
            return hdrLines.ToArray();
        }

        private string RemoveLineEndings(string str)
        {
            return Regex.Replace(str, @"\r\n|\r|\n", "; ");
        }
    }

    public enum HdrType
    {
        Unkown,
        Simple,
        Compound
    }
}
