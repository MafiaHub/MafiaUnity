using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MafiaUnity
{
    namespace MafiaFormats
    {
        public class TckLoader : BaseLoader
        {
            public class Header
            {
                public uint magicByte;              // 0x4 - determines .tck file
                public Vector3 startPosition;           // TODO: find out what it stands for - maybe a bounding box ?
                public Vector3 endPosition;
                public uint lengthOfAnimation;
                public uint milisecondsPerFrame;      // frequency ?
                public uint countOfPositionBlocks;    // verified 
            }

            public class PositionTransform
            {
                public Vector3 position;
            }

            public Header header;
            public List<PositionTransform> transforms;

            Header ReadHeader(BinaryReader reader)
            {
                Header newHeader = new Header();
                newHeader.magicByte = reader.ReadUInt32();
                newHeader.startPosition = ReadVector3(reader);
                newHeader.endPosition = ReadVector3(reader);

                newHeader.lengthOfAnimation = reader.ReadUInt32();
                newHeader.milisecondsPerFrame = reader.ReadUInt32();
                newHeader.countOfPositionBlocks = reader.ReadUInt32();
                
                return newHeader;
            }

            public void load(BinaryReader reader)
            {
                this.header = this.ReadHeader(reader);
               

                if (this.header.magicByte != 0x04)
                {
                    Debug.LogError("Unable to parse TCK File - wrong header!");
                    return;
                }


                PositionTransform chunk = new PositionTransform();
                for (var i = 0; i < this.header.countOfPositionBlocks; i++)
                {
                    chunk.position = ReadVector3(reader);
                    transforms.Add(chunk);
                }
            }
        }
    }
}