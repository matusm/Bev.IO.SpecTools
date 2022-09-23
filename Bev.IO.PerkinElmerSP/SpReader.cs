﻿using Bev.IO.SpectrumPod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bev.IO.PerkinElmerSP
{
    public class SpReader
    {
        private const BlockCodes MainBlock = BlockCodes.DSet2DC1DI;
        private const int DataMemberDataOffset = 4;
        private const int SizeofDouble = 8;

        private BlockFile blockFile;
        private List<TypedMemberBlock> memberBlocks = new List<TypedMemberBlock>();
        private double[] pointsY;

        public Spectrum Spectrum { get; private set; }
        public string[] HdrHistory { get; private set; }

        public uint SPCheckSum { get; private set; }
        public int SPNumPoints { get; private set; }
        public UInt16 SPDataType { get; private set; }
        public double SPResolutionX { get; private set; }
        public double SPStartX { get; private set; }
        public double SPEndX { get; private set; }
        public double SPMinY { get; private set; }
        public double SPMaxY { get; private set; }
        public string SPName { get; private set; }
        public string SPAlias { get; private set; }
        public string SPLabelX { get; private set; }
        public string SPLabelY { get; private set; }
        public string SPFileType { get; private set; }
        public string SPSampling { get; private set; }


        public SpReader(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open);
            blockFile = new BlockFile(fileStream);
            AnalyseMainBlock(blockFile);
            InterpretKnownBlocks();
            Spectrum = new Spectrum();
            Spectrum.SourceFileName = Path.GetFileName(path);
            Spectrum.SourceFileCreationDate = File.GetCreationTimeUtc(path);
            BuildSpectrumPod();
        }

        public void DebugOutput()
        {
            Console.WriteLine("============================================================");
            foreach (var block in blockFile.Contents)
                Console.WriteLine(block);
            var data = blockFile.Contents.Last().Data;
            Console.WriteLine($"last block data: {BytesToHex(data)}");
            Console.WriteLine("============================================================");
            foreach (var typedBlock in memberBlocks)
                Console.WriteLine(typedBlock);
            Console.WriteLine("============================================================");
            Console.WriteLine($"Checksum:   {SPCheckSum}");
            //Console.WriteLine($"NumPoints:  {SPNumPoints}");
            //Console.WriteLine($"Resolution: {SPResolutionX}");
            //Console.WriteLine($"StartX:     {SPStartX}");
            //Console.WriteLine($"EndX:       {SPEndX}");
            //Console.WriteLine($"MinY:       {SPMinY}");
            //Console.WriteLine($"MaxY:       {SPMaxY}");
            Console.WriteLine($"Name:       {SPName}");
            Console.WriteLine($"Alias:      {SPAlias}");
            Console.WriteLine($"LabelX:     {SPLabelX}");
            Console.WriteLine($"LabelY:     {SPLabelY}");
            Console.WriteLine($"FileType:   {SPFileType}");
            Console.WriteLine($"Sampling:   {SPSampling}");
            Console.WriteLine($"DataType:   {SPDataType}");
            Console.WriteLine("============================================================");
            for (int i = 0; i < HdrHistory.Length; i++)
            {
                Console.WriteLine($"{i,2} : >{HdrHistory[i]}<");
            }
            Console.WriteLine("============================================================");
        }

        private void InterpretKnownBlocks()
        {
            foreach (TypedMemberBlock tmb in memberBlocks)
                InterpretBlock(tmb);
        }

        private void InterpretBlock(TypedMemberBlock tmb)
        {
            switch ((BlockCodes)tmb.Id)
            {
                case BlockCodes.DataSetDataType:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.UInt)
                        SPDataType = BitConverter.ToUInt16(tmb.Data, 0);
                    break;
                case BlockCodes.DataSetAbscissaRange:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.CvCoOrdRange)
                    {
                        SPStartX = BitConverter.ToDouble(tmb.Data, 0);
                        SPEndX = BitConverter.ToDouble(tmb.Data, SizeofDouble);
                    }
                    break;
                case BlockCodes.DataSetOrdinateRange:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.CvCoOrdRange)
                    {
                        SPMinY = BitConverter.ToDouble(tmb.Data, 0);
                        SPMaxY = BitConverter.ToDouble(tmb.Data, SizeofDouble);
                    }
                    break;
                case BlockCodes.DataSetInterval:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.CvCoOrd)
                        SPResolutionX = BitConverter.ToDouble(tmb.Data, 0);
                    break;
                case BlockCodes.DataSetNumPoints:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Long)
                        SPNumPoints = BitConverter.ToInt32(tmb.Data, 0);
                    break;
                case BlockCodes.DataSetSamplingMethod:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        SPSampling = BytesToString(tmb.Data);
                    break;
                case BlockCodes.DataSetXAxisLabel:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        SPLabelX = BytesToString(tmb.Data);
                    break;
                case BlockCodes.DataSetYAxisLabel:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        SPLabelY = BytesToString(tmb.Data);
                    break;
                case BlockCodes.DataSetXAxisUnitType:
                    //Console.WriteLine(ToPrettyString(tmb.Data));
                    break;
                case BlockCodes.DataSetYAxisUnitType:
                    //Console.WriteLine(ToPrettyString(tmb.Data));
                    break;
                case BlockCodes.DataSetFileType:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        SPFileType = BytesToString(tmb.Data);
                    break;
                case BlockCodes.DataSetData:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.CvCoOrdArray)
                    {
                        if (pointsY == null)
                            pointsY = new double[BitConverter.ToInt32(tmb.Data, 0) / SizeofDouble];
                        try
                        {
                            for (int i = 0; i < pointsY.Length; i++)
                            {
                                pointsY[i] = BitConverter.ToDouble(tmb.Data, DataMemberDataOffset + i * SizeofDouble);
                            }
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine("Warning: an unexpected end of data member block has been encountered.");
                        }
                    }
                    break;
                case BlockCodes.DataSetName:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        SPName = BytesToString(tmb.Data);
                    break;
                case BlockCodes.DataSetChecksum:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.ULong)
                        SPCheckSum = BitConverter.ToUInt32(tmb.Data, 0);
                    break;
                case BlockCodes.DataSetHistoryRecord:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.InstrHdrHistoryRecord)
                        HdrHistory = _XYZZY(tmb.Data);
                    break;
                case BlockCodes.DataSetInvalidRegion:
                    break;
                case BlockCodes.DataSetAlias:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        SPAlias = BytesToString(tmb.Data);
                    break;
                case BlockCodes.DataSetVXIRAccyHdr:
                    break;
                case BlockCodes.DataSetVXIRQualHdr:
                    break;
                case BlockCodes.DataSetEventMarkers:
                    break;
                default:
                    break;
            }
        }

        private void BuildSpectrumPod()
        {
            double x = SPStartX;
            foreach (var y in pointsY)
            {
                Spectrum.AddDataValue(x, y);
                x += SPResolutionX;
            }
            Spectrum.SetUnitNames(SPLabelX, SPLabelY);
            Spectrum.AddMetaData("SPName", SPName);
            Spectrum.AddMetaData("SPAlias", SPAlias);
            Spectrum.AddMetaData("Description", blockFile.Description);
            Spectrum.AddMetaData("SPDataType", SPDataType.ToString());
            Spectrum.AddMetaData("SPFileType", SPFileType);
            Spectrum.AddMetaData("SPSampling", SPSampling);
        }

        private string[] _XYZZY(byte[] data)
        {
            if (data.Length < 5) return null;
            List<string> hdrLines = new List<string>();
            for (int i = 1; i < data.Length-1; i++)
            {
                if(data[i-1] == 0x23 && data[i] == 0x75)
                {
                    int len = BitConverter.ToInt16(data, i+1);
                    hdrLines.Add(Encoding.ASCII.GetString(data, i+3, len));
                }
            }
            return hdrLines.ToArray();
        }

        private void AnalyseMainBlock(BlockFile blockFile)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian architectures are not supported (yet).");
            Block main = blockFile.Contents.FirstOrDefault(x => x.Id == (short)MainBlock);
            if (main == null)
                throw new NotSupportedException($"This SP file doesn't contain a {Enum.GetName(typeof(BlockCodes), MainBlock)} block.");
            SplitToMemberBlocks(main.Data);
        }

        private void SplitToMemberBlocks(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);
            using BinaryReader binReader = new BinaryReader(ms);
            while (binReader.BaseStream.Position < binReader.BaseStream.Length)
            {
                TypedMemberBlock tmb = null;
                try
                {
                    tmb = new TypedMemberBlock(binReader);
                    memberBlocks.Add(tmb);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }
        }

        private string BytesToString(byte[] data)
        {
            try
            {
                int len = BitConverter.ToInt16(data, 0);
                return Encoding.ASCII.GetString(data, 2, len);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Warning: couldn't read a string field due to a bad length value.");
                return null;
            }
        }

        private string BytesToHex(byte[] data)
        {
            string str = "";
            foreach (var b in data)
                str += $"{b:X2} ";
            return str;
        }

    }
}
