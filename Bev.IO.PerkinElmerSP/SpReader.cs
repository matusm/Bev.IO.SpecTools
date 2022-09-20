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

        public SpReader(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open);
            blockFile = new BlockFile(fileStream);
            AnalyseMainBlock(blockFile);
        }

        public void DebugOutput()
        {
            Console.WriteLine("============================================================");
            foreach (var block in blockFile.Contents)
            {
                Console.WriteLine(block);
            }
            Console.WriteLine("============================================================");
            foreach (var typedBlock in memberBlocks)
            {
                Console.WriteLine(typedBlock);
            }
            Console.WriteLine("============================================================");
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



    }
}
