using System.IO;

namespace Bev.IO.PerkinElmerSP
{
    public class TypedMemberBlock : Block
    {
        public short TypeCode { get; }

        public TypedMemberBlock(BinaryReader file) : base(file.ReadInt16())
        {
            int len = file.ReadInt32();
            TypeCode = file.ReadInt16();
            Data = file.ReadBytes(len - 2);
        }
    }
}
