using Bev.IO.SpectrumPod;
using System;
using System.Collections.Generic;
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
            spectrum.AddMetaData("Title", GetTitle());
            spectrum.AddMetaData("Owner", GetUser());
            spectrum.AddMetaData("SampleDescription", GetTitle());
            spectrum.AddMetaData("SpectrometerModel", GetInstrument());
            spectrum.AddMetaData("SpectrometerSerialNumber", GetInstrumentSN());
            spectrum.AddMetaData("SoftwareID", GetSoftwareVersion());
            spectrum.AddMetaData("InstrumentParameters", GetInstrumentParameters());
            spectrum.AddMetaData("Comments", GetComments());
            spectrum.AddMetaData("Bandpass", GetBandpass());
            spectrum.AddMetaData("DetectorChange", GetDetectorChange()); // monochromator/detector change
            spectrum.AddMetaData("SampleBeamPosition", GetBeamPosition()); // sample beam position
            spectrum.AddMetaData("CommonBeamDepolarizer", GetCBD()); // common beam depolarizer ?
            spectrum.AddMetaData("Attenuators", GetAttenuators()); // attenuators
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

        private string GetUser() => GetHdrHistoryLine(0);
        private string GetTitle() => GetHdrHistoryLine(4);
        private string GetInstrument() => GetHdrHistoryLine(5);
        private string GetInstrumentSN() => GetHdrHistoryLine(6);
        private string GetSoftwareVersion() => GetHdrHistoryLine(7);
        private string GetComments() => GetHdrHistoryLine(8);
        private string GetAttenuators() => HdrHistory[HdrHistory.Length - 14];
        private string GetCBD() => HdrHistory[HdrHistory.Length - 15];
        private string GetBeamPosition() => HdrHistory[HdrHistory.Length - 16];
        private string GetDetectorChange() => HdrHistory[HdrHistory.Length - 17];
        private string GetParameter0() => HdrHistory[HdrHistory.Length - 22];
        private string GetInstrumentParameters()
        {
            int idx = FindUvVisIndex();
            if (idx < 0) return string.Empty;
            return GetHdrHistoryLine(idx + 1);
        }
        private string GetBandpass()
        {
            int idx = FindUvVisIndex();
            if (idx < 0) return string.Empty;
            return GetHdrHistoryLine(idx - 1);
        }

        private string GetHdrHistoryLine(int lineNumber)
        {
            int corLineNumber = lineNumber + 5 * NumberOfSubblocks();
            if (corLineNumber < 0 || corLineNumber >= HdrHistory.Length)
                return string.Empty;
            return HdrHistory[corLineNumber];
        }

        private int FindUvVisIndex()
        {
            for (int i = 0; i < 40; i++)
            {
                if (GetHdrHistoryLine(i)=="UV/VIS")
                {
                    return i;
                }
            }
            return -1;
        }

        private int NumberOfSubblocks()
        {
            int minLength = 36;
            if(HdrHistory.Length > minLength)
            {
                return (HdrHistory.Length - minLength) /5;
            }
            return 0;
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
