using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    public class TextDBLoader : BaseLoader
    {

        public unsafe struct Header
        {
            public uint numStrings;
            public uint unknown;
        }

        public unsafe struct TextBlock
        {
            public uint textId;
            public uint textOffset;
        }

        public List<TextBlock> textBlocks = new List<TextBlock>();
        public Dictionary<uint, string> textEntries = new Dictionary<uint, string>();

        //TODO(DavoSK): Move all those function to utils 
        private int peekLength(BinaryReader reader)
        {
            int currentSize = 0;
            long currentPos = reader.BaseStream.Position;
            byte curentChar = 1;

            while (curentChar != 0)
            {
                curentChar = reader.ReadByte();
                currentSize++;
            }
            reader.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return currentSize;
        }

        public void Load(BinaryReader reader)
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
                var textLen = peekLength(reader);
                textEntries.Add(textBlock.textId, new string(reader.ReadChars(textLen)));
            }
        }
    }
}
