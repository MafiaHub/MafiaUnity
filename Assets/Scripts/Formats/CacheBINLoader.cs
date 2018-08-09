using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    namespace MafiaFormats
    {
        public class CacheBINLoader : BaseLoader
        {
            public class Header
            {
                public ushort type;
                public uint size;
            }

            public class Instance
            {
                public Header header;
                public string modelName;
                public Vector3 pos;
                public Quaternion rot;
                public Vector3 scale;
                public uint unk0;
                public Vector3 scale2;
            }

            public class Object
            {
                public Header header;
                public string objectName;
                public byte[] bounds; // 0x4C
                public List<Instance> instances;
            }

            public class Chunk
            {
                public uint version;
            }

            public List<Object> objects;
            
            Header ReadHeader(BinaryReader reader)
            {
                Header newHeader = new Header();
                newHeader.type = reader.ReadUInt16();
                newHeader.size = reader.ReadUInt32();
                return newHeader;
            }

            public void ReadCache(BinaryReader reader)
            {
                if (objects == null)
                    objects = new List<Object>();
                
                Header newHeader = ReadHeader(reader);
                Chunk newChunk = new Chunk();
                newChunk.version = reader.ReadUInt32();

                while (reader.BaseStream.Position < newHeader.size - sizeof(uint))
                {
                    Object newObject = new Object();
                    newObject.header = ReadHeader(reader);

                    var objectNameLength = reader.ReadUInt32();
                    newObject.objectName = new string(reader.ReadChars((int)objectNameLength));

                    newObject.bounds = new byte[0x4C];
                    for (var i = 0; i < 0x4C; i++)
                        newObject.bounds[i] = reader.ReadByte();

                    var currentPos = reader.BaseStream.Position;
                    var headerSize = sizeof(ushort) + sizeof(uint) + sizeof(uint) + objectNameLength + 0x4C;

                    newObject.instances = new List<Instance>();

                    while (reader.BaseStream.Position < currentPos + newObject.header.size - headerSize)
                    {
                        Instance newInstance = new Instance();
                        newInstance.header = ReadHeader(reader);

                        //NOTE(DavoSK): renaming .i3ds to 4ds
                        var modelNameLength = reader.ReadUInt32();
                        newInstance.modelName = new string(reader.ReadChars((int)modelNameLength)).Replace(".i3d", ".4ds");

                        newInstance.pos = ReadVector3(reader);
                        newInstance.rot = ReadQuat(reader);

                        var rot = newInstance.rot;
                        var tmpRot = new Quaternion(rot.y, rot.z, rot.w, -1 * rot.x);
                        newInstance.rot = tmpRot;

                        newInstance.scale = ReadVector3(reader);
                        newInstance.unk0 = reader.ReadUInt32();
                        newInstance.scale2 = ReadVector3(reader);

                        newObject.instances.Add(newInstance);
                    }

                    objects.Add(newObject);
                }
            }
        }
    }
}