using System.IO;

namespace Bev.IO.PerkinElmerSP
{
    public class TypedBlock : Block
    {
        public short TypeCode { get; }

        public TypedBlock(BinaryReader binReader) : base(binReader.ReadInt16())
        {
            int len = binReader.ReadInt32();
            TypeCode = binReader.ReadInt16();
            Data = binReader.ReadBytes(len - 2);
        }

        public override string ToString() => $"TypedBlock[{(BlockCodes)Id} {(BlockCodes)TypeCode} {Data.Length}]";
    }
}
