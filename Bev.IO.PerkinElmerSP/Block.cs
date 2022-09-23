using System.IO;

namespace Bev.IO.PerkinElmerSP
{
    /// <summary>
    /// Each block contains fields:
    /// Id (int16), 
    /// Length (int32), 
    /// [for "member"-blocks only: innerCode (int16) = data type], 
    /// Data (arbitrary).
    /// For *.SP files "member-blocks" are considered as data of "wrapper-blocks"
    /// </summary>
    /// <seealso cref="TypedMemberBlock"/>
    public class Block
    {

        public short Id { get; }
        public byte[] Data { get; protected set; }

        public Block(short id)
        {
            Id = id;
        }

        public Block(BinaryReader binReader)
        {
            Id = binReader.ReadInt16();
            int len = binReader.ReadInt32();
            Data = binReader.ReadBytes(len);
            if (Data.Length < len) throw new EndOfStreamException();
        }

        public override string ToString() => $"Block[{(BlockCodes)Id} {Data.Length}]";
    }
}
