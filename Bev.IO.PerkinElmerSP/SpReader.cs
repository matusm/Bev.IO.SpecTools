using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bev.IO.PerkinElmerSP
{
    public class SpReader
    {
        private const BlockCodes MainBlock = BlockCodes.DSet2DC1DI;
        private const int DataMemberDataOffset = 4;
        private const int SizeofDouble = 8;

        private BlockFile blockFile;
        private List<TypedMemberBlock> memberBlocks = new List<TypedMemberBlock>();

        public uint CheckSum { get; private set; }
        public double ResolutionX { get; private set; }
        public double StartX { get; private set; }
        public double EndX { get; private set; }
        public double MinY { get; private set; }
        public double MaxY { get; private set; }
        public string Name { get; private set; }
        public string Alias { get; private set; }
        public int NumPoints { get; private set; }
        public UInt16 DataType { get; private set; }
        public string LabelX { get; private set; }
        public string LabelY { get; private set; }
        public string FileType { get; private set; }
        public string Sampling { get; private set; }


        public SpReader(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open);
            blockFile = new BlockFile(fileStream);
            AnalyseMainBlock(blockFile);
            Interpreter();
        }

        public void DebugOutput()
        {
            Console.WriteLine("============================================================");
            Console.WriteLine($"Description: {blockFile.Description}");
            foreach (var block in blockFile.Contents)
                Console.WriteLine(block);
            var data = blockFile.Contents.Last().Data;
            Console.WriteLine($"last block data: {ToPrettyString(data)}");
            Console.WriteLine("============================================================");
            foreach (var typedBlock in memberBlocks)
                Console.WriteLine(typedBlock);
            Console.WriteLine("============================================================");
            Console.WriteLine($"Checksum:   {CheckSum}");
            Console.WriteLine($"NumPoints:  {NumPoints}");
            Console.WriteLine($"Resolution: {ResolutionX}");
            Console.WriteLine($"StartX:     {StartX}");
            Console.WriteLine($"EndX:       {EndX}");
            Console.WriteLine($"MinY:       {MinY}");
            Console.WriteLine($"MaxY:       {MaxY}");
            Console.WriteLine($"Name:       {Name}"); 
            Console.WriteLine($"Alias:      {Alias}");
            Console.WriteLine($"LabelX:     {LabelX}");
            Console.WriteLine($"LabelY:     {LabelY}");
            Console.WriteLine($"FileType:   {FileType}");
            Console.WriteLine($"Sampling:   {Sampling}");
            Console.WriteLine($"DataType:   {DataType}");
            Console.WriteLine("============================================================");
        }

        private void Interpreter()
        {
            foreach (TypedMemberBlock tmb in memberBlocks)
                Interpreter(tmb);
        }

        private void Interpreter(TypedMemberBlock tmb)
        {
            switch ((BlockCodes)tmb.Id)
            {
                case BlockCodes.DataSetDataType:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.UInt)
                        DataType = BitConverter.ToUInt16(tmb.Data, 0);
                    break;
                case BlockCodes.DataSetAbscissaRange:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.CvCoOrdRange)
                    {
                        StartX = BitConverter.ToDouble(tmb.Data, 0);
                        EndX = BitConverter.ToDouble(tmb.Data, SizeofDouble);
                    }
                    break;
                case BlockCodes.DataSetOrdinateRange:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.CvCoOrdRange)
                    {
                        MinY = BitConverter.ToDouble(tmb.Data, 0);
                        MaxY = BitConverter.ToDouble(tmb.Data, SizeofDouble);
                    }
                    break;
                case BlockCodes.DataSetInterval:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.CvCoOrd)
                        ResolutionX = BitConverter.ToDouble(tmb.Data, 0);
                    break;
                case BlockCodes.DataSetNumPoints:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Long)
                        NumPoints = BitConverter.ToInt32(tmb.Data, 0);
                    break;
                case BlockCodes.DataSetSamplingMethod:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        Sampling = ReadString(tmb.Data);
                    break;
                case BlockCodes.DataSetXAxisLabel:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        LabelX = ReadString(tmb.Data);
                    break;
                case BlockCodes.DataSetYAxisLabel:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        LabelY = ReadString(tmb.Data);
                    break;
                case BlockCodes.DataSetXAxisUnitType:
                    //Console.WriteLine(ToPrettyString(tmb.Data));
                    break;
                case BlockCodes.DataSetYAxisUnitType:
                    //Console.WriteLine(ToPrettyString(tmb.Data));
                    break;
                case BlockCodes.DataSetFileType:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        FileType = ReadString(tmb.Data);
                    break;
                case BlockCodes.DataSetData:
                    break;
                case BlockCodes.DataSetName:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        Name = ReadString(tmb.Data);
                    break;
                case BlockCodes.DataSetChecksum:
                    if((BlockCodes)tmb.TypeCode == BlockCodes.ULong)
                        CheckSum = BitConverter.ToUInt32(tmb.Data, 0);
                    break;
                case BlockCodes.DataSetHistoryRecord:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.InstrHdrHistoryRecord)
                        XYZZY(tmb.Data); // TODO does not work !?
                    break;
                case BlockCodes.DataSetInvalidRegion:
                    break;
                case BlockCodes.DataSetAlias:
                    if ((BlockCodes)tmb.TypeCode == BlockCodes.Char)
                        Alias = ReadString(tmb.Data);
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

        private void XYZZY(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);
            using BinaryReader binReader = new BinaryReader(ms);
            while (binReader.BaseStream.Position < binReader.BaseStream.Length)
            {
                Block tmb = null;
                try
                {
                    tmb = new Block(binReader);
                    Console.WriteLine(">>>>" + tmb);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }
        }

        private void AnalyseMainBlock(BlockFile blockFile)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian architectures are not supported (yet).");
            Block main = blockFile.Contents.FirstOrDefault(x => x.Id == (short)MainBlock);
            if (main == null)
                throw new NotSupportedException($"This SP file doesn't contain a {Enum.GetName(typeof(BlockCodes), MainBlock)} block.");
            ParseAndAddMembers(main.Data);
        }

        private void ParseAndAddMembers(byte[] data)
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

        private string ReadString(byte[] data)
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

        private string ToPrettyString(byte[] data)
        {
            string str = "";
            foreach (var b in data)
                str += $"{b:X2} ";
            return str;
        }

    }
}
