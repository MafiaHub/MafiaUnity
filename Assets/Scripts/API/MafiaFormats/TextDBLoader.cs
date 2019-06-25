using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MafiaFormats
{
    public partial class TextDBLoader : BaseLoader
    {
        public struct Header
        {
            public uint numStrings;
            public uint unknown;
        }

        public struct TextBlock
        {
            public uint textId;
            public uint textOffset;
        }

        public List<TextBlock> textBlocks = new List<TextBlock>();
        public Dictionary<uint, string> textEntries = new Dictionary<uint, string>();

        public bool Load(BinaryReader reader)
        {
            Header newHeader = new Header();
            newHeader.numStrings = reader.ReadUInt32();
            newHeader.unknown = reader.ReadUInt32();

            for (var i = 0; i < newHeader.numStrings; i++)
            {
                TextBlock newTextBlock = new TextBlock();
                newTextBlock.textId = reader.ReadUInt32();
                newTextBlock.textOffset = reader.ReadUInt32();

                textBlocks.Add(newTextBlock);
            }

            foreach (var textBlock in textBlocks)
            {
                reader.BaseStream.Seek(textBlock.textOffset, SeekOrigin.Begin);
                textEntries.Add(textBlock.textId, ReadTerminatedString(reader));
            }

            return true;
        }
    }
}
