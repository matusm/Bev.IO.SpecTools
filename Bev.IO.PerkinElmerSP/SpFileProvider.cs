using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bev.IO.PerkinElmerSP
{
    public class SpFileProvider
    {
        private const BlockCodes MainBlock = BlockCodes.DSet2DC1DI;
        private const int DataMemberDataOffset = 4;
        private const int SizeofDouble = 8;


        private static SpFileProvider _instance = new SpFileProvider();
        private SpFileProvider() { }
        public static SpFileProvider Instance { get => _instance; }
        public string Extension { get; } = ".sp";

        static string ReadString(byte[] data)
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

        static void GetSpectrumWrapper(TypedMemberBlock mb, Spectrum2d sp)
        {
            switch ((BlockCodes)mb.Id)
            {
                case BlockCodes.DataSetAbscissaRange:
                    if (mb.TypeCode != (short)BlockCodes.CvCoOrdRange)
                        throw new NotSupportedException("Not supported data type for X axis range.");
                    sp.StartX = BitConverter.ToDouble(mb.Data, 0);
                    sp.EndX = BitConverter.ToDouble(mb.Data, SizeofDouble);
                    break;
                case BlockCodes.DataSetInterval:
                    sp.ResolutionX = BitConverter.ToDouble(mb.Data, 0);
                    break;
                case BlockCodes.DataSetNumPoints:
                    sp.PointsY = new double[BitConverter.ToInt32(mb.Data, 0)];
                    break;
                case BlockCodes.DataSetXAxisLabel:
                    sp.LabelX = ReadString(mb.Data);
                    break;
                case BlockCodes.DataSetYAxisLabel:
                    sp.LabelY = ReadString(mb.Data);
                    break;
                case BlockCodes.DataSetData:
                    if (mb.TypeCode != (short)BlockCodes.CvCoOrdArray)
                        throw new NotSupportedException("Not supported data type for Y data array.");
                    if (sp.PointsY == null)
                        sp.PointsY = new double[BitConverter.ToInt32(mb.Data, 0) / SizeofDouble];
                    try
                    {
                        for (int i = 0; i < sp.PointsY.Length; i++)
                        {
                            sp.PointsY[i] = BitConverter.ToDouble(mb.Data, DataMemberDataOffset + i * SizeofDouble);
                        }
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Warning: an unexpected end of data member block has been encountered.");
                    }
                    break;
                case BlockCodes.DataSetName:
                    sp.Name = ReadString(mb.Data);
                    break;
                case BlockCodes.DataSetAlias:
                    sp.Alias = ReadString(mb.Data);
                    break;
                default:
                    Console.WriteLine($"Info: ignoring unknown block id {mb.Id} : {(BlockCodes)mb.Id}.");
                    break;
            }
        }

        static IEnumerable<TypedMemberBlock> ParseMembers(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);
            using BinaryReader r = new BinaryReader(ms);
            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                TypedMemberBlock b = null;
                try
                {
                    b = new TypedMemberBlock(r);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                yield return b;
            }
        }

        public IData GetData(BlockFile file)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian architectures are not supported (yet).");
            Block main = file.Contents.FirstOrDefault(x => x.Id == (short)MainBlock);
            if (main == null)
                throw new NotSupportedException($"This SP file doesn't contain a {Enum.GetName(typeof(BlockCodes), MainBlock)} block.");
            var spec = new Spectrum2d();
            foreach (var item in ParseMembers(main.Data))
            {
                GetSpectrumWrapper(item, spec);
            }
            return spec;
        }


    }
}
