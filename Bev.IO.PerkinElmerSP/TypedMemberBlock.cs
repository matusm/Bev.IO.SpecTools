using System.IO;

namespace Bev.IO.PerkinElmerSP
{
    public class TypedMemberBlock : Block
    {
        public short TypeCode { get; }

        public TypedMemberBlock(BinaryReader binReader) : base(binReader.ReadInt16())
        {
            int len = binReader.ReadInt32();
            TypeCode = binReader.ReadInt16();
            Data = binReader.ReadBytes(len - 2);
        }
    }
}
