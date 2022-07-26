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

        private readonly BlockFile blockFile;
        private readonly List<TypedBlock> memberBlocks = new List<TypedBlock>();
        private double[] pointsY;   // temporary place for the spectrum

        public Spectrum Spectrum { get; private set; }
        public HdrHistoryParser History { get; private set; }
        public string FileName { get; }
        public DateTime FileCreationDate { get; }

        // TODO mak all private

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
            FileName = Path.GetFileName(path);
            FileCreationDate = File.GetCreationTimeUtc(path);
            blockFile = new BlockFile(fileStream);
            AnalyseMainBlock(blockFile);
            InterpretKnownTypedBlocks();
            Spectrum = new Spectrum();
            Spectrum.SourceFileName = FileName;
            Spectrum.SourceFileCreationDate = FileCreationDate;
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
        }

        private void InterpretKnownTypedBlocks()
        {
            foreach (TypedBlock tb in memberBlocks)
                InterpretBlock(tb);
        }

        private void InterpretBlock(TypedBlock tb)
        {
            switch ((BlockCodes)tb.Id)
            {
                case BlockCodes.DataSetDataType:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.UInt)
                        SPDataType = BitConverter.ToUInt16(tb.Data, 0);
                    break;
                case BlockCodes.DataSetAbscissaRange:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.CvCoOrdRange)
                    {
                        SPStartX = BitConverter.ToDouble(tb.Data, 0);
                        SPEndX = BitConverter.ToDouble(tb.Data, SizeofDouble);
                    }
                    break;
                case BlockCodes.DataSetOrdinateRange:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.CvCoOrdRange)
                    {
                        SPMinY = BitConverter.ToDouble(tb.Data, 0);
                        SPMaxY = BitConverter.ToDouble(tb.Data, SizeofDouble);
                    }
                    break;
                case BlockCodes.DataSetInterval:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.CvCoOrd)
                        SPResolutionX = BitConverter.ToDouble(tb.Data, 0);
                    break;
                case BlockCodes.DataSetNumPoints:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.Long)
                        SPNumPoints = BitConverter.ToInt32(tb.Data, 0);
                    break;
                case BlockCodes.DataSetSamplingMethod:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.Char)
                        SPSampling = BytesToString(tb.Data);
                    break;
                case BlockCodes.DataSetXAxisLabel:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.Char)
                        SPLabelX = BytesToString(tb.Data);
                    break;
                case BlockCodes.DataSetYAxisLabel:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.Char)
                        SPLabelY = BytesToString(tb.Data);
                    break;
                case BlockCodes.DataSetXAxisUnitType:
                    //Console.WriteLine(ToPrettyString(tmb.Data));
                    break;
                case BlockCodes.DataSetYAxisUnitType:
                    //Console.WriteLine(ToPrettyString(tmb.Data));
                    break;
                case BlockCodes.DataSetFileType:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.Char)
                        SPFileType = BytesToString(tb.Data);
                    break;
                case BlockCodes.DataSetData:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.CvCoOrdArray)
                    {
                        if (pointsY == null)
                            pointsY = new double[BitConverter.ToInt32(tb.Data, 0) / SizeofDouble];
                        try
                        {
                            for (int i = 0; i < pointsY.Length; i++)
                            {
                                pointsY[i] = BitConverter.ToDouble(tb.Data, DataMemberDataOffset + i * SizeofDouble);
                            }
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine("Warning: an unexpected end of data member block has been encountered.");
                        }
                    }
                    break;
                case BlockCodes.DataSetName:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.Char)
                        SPName = BytesToString(tb.Data);
                    break;
                case BlockCodes.DataSetChecksum:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.ULong)
                        SPCheckSum = BitConverter.ToUInt32(tb.Data, 0);
                    break;
                case BlockCodes.DataSetHistoryRecord:
                    History = new HdrHistoryParser(tb);
                    break;
                case BlockCodes.DataSetInvalidRegion:
                    break;
                case BlockCodes.DataSetAlias:
                    if ((BlockCodes)tb.TypeCode == BlockCodes.Char)
                        SPAlias = BytesToString(tb.Data);
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
            Spectrum.AddMetaData("SpName", SPName);
            Spectrum.AddMetaData("SpAlias", SPAlias);
            Spectrum.AddMetaData("SpDescription", blockFile.Description);
            Spectrum.AddMetaData("SpDataType", SPDataType.ToString());
            Spectrum.AddMetaData("SpFileType", SPFileType);
            Spectrum.AddMetaData("SpSampling", SPSampling);
            Spectrum.AddMetaData("SpChecksum", SPCheckSum.ToString());
            Spectrum.AddMetaData(History.HdrHistoryDict);
        }

        private void AnalyseMainBlock(BlockFile blockFile)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian architectures are not supported (yet).");
            Block main = blockFile.Contents.FirstOrDefault(x => x.Id == (short)MainBlock);
            if (main == null)
                throw new NotSupportedException($"This SP file doesn't contain a {Enum.GetName(typeof(BlockCodes), MainBlock)} block.");
            SplitToTypedBlocks(main.Data);
        }

        private void SplitToTypedBlocks(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);
            using BinaryReader binReader = new BinaryReader(ms);
            while (binReader.BaseStream.Position < binReader.BaseStream.Length)
            {
                TypedBlock tb = null;
                try
                {
                    tb = new TypedBlock(binReader);
                    memberBlocks.Add(tb);
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
