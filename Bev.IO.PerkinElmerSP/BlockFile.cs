using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bev.IO.PerkinElmerSP
{
    public class  BlockFile
    {
        private const string FileSignature = "PEPE";
        private const int DescriptionRecordLength = 40;

        public string Description { get; }
        public Block[] Contents { get; }

        public BlockFile(FileStream file)
        {
            //Parse header
            byte[] signature = new byte[FileSignature.Length];
            file.Read(signature, 0, signature.Length);
            if (Encoding.ASCII.GetString(signature) != FileSignature)
                throw new NotSupportedException("This is not a Perkin-Elmer block file.");
            byte[] description = new byte[DescriptionRecordLength];
            file.Read(description, 0, description.Length);
            Description = Encoding.ASCII.GetString(description);
            //Read contents
            List<Block> blocks = new List<Block>(); //Todo: some capacity heuristics based on file length?
            try
            {
                using (BinaryReader binaryReader = new BinaryReader(file))
                {
                    while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                    {
                        blocks.Add(new Block(binaryReader));
                    }
                }
            }
            catch (EndOfStreamException)
            { }
            Contents = blocks.ToArray();
        }

    }
}
