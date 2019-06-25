using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            public Chunk chunk;
            
            Header ReadHeader(BinaryReader reader)
            {
                Header newHeader = new Header();
                newHeader.type = reader.ReadUInt16();
                newHeader.size = reader.ReadUInt32();
                return newHeader;
            }

            void WriteHeader(BinaryWriter writer, Header header)
            {
                writer.Write(header.type);
                writer.Write(header.size);
            }

            void WriteChunk(BinaryWriter writer, Chunk chunk)
            {
                writer.Write(chunk.version);
            }

            void WriteObject(BinaryWriter writer, Object obj)
            {
                WriteHeader(writer, obj.header);

                writer.Write((int)obj.objectName.Length);
                writer.Write(obj.objectName);
                writer.Write(obj.bounds);
                
            }

            public void WriteCache(string path)
            {
                path = GameAPI.instance.fileSystem.GetPath(path);

                var fs = new FileStream(path, FileMode.Create);

                using (var writer = new BinaryWriter(fs))
                {
                    Header newHeader = new Header();
                    newHeader.size = (uint)objects.Sum(x => x.header.size);

                    WriteHeader(writer, newHeader);
                    WriteChunk(writer, chunk);

                    foreach (var obj in objects)
                    {
                        WriteHeader(writer, obj.header);

                        WriteStringUInt32(writer, obj.objectName);
                        writer.Write(obj.bounds);

                        foreach (var inst in obj.instances)
                        {
                            WriteHeader(writer, inst.header);
                            WriteStringUInt32(writer, inst.modelName.Replace(".4ds", ".i3d"));
                            WriteVector3(writer, inst.pos);
                            WriteQuat(writer, new Quaternion(-1 * inst.rot.w, inst.rot.x, inst.rot.y, inst.rot.z));
                            WriteVector3(writer, inst.scale);
                            writer.Write(inst.unk0);
                            WriteVector3(writer, inst.scale2);
                        }
                    }
                }

                fs.Close();
            }

            public void ReadCache(BinaryReader reader)
            {
                if (objects == null)
                    objects = new List<Object>();
                
                Header newHeader = ReadHeader(reader);
                chunk = new Chunk();
                chunk.version = reader.ReadUInt32();

                while (reader.BaseStream.Position < newHeader.size - sizeof(uint))
                {
                    Object newObject = new Object();
                    newObject.header = ReadHeader(reader);

                    newObject.objectName = ReadStringUInt32(reader);

                    newObject.bounds = new byte[0x4C];
                    for (var i = 0; i < 0x4C; i++)
                        newObject.bounds[i] = reader.ReadByte();

                    var currentPos = reader.BaseStream.Position;
                    var headerSize = sizeof(ushort) + sizeof(uint)*2 + newObject.objectName.Length + 0x4C;

                    newObject.instances = new List<Instance>();

                    while (reader.BaseStream.Position < currentPos + newObject.header.size - headerSize)
                    {
                        Instance newInstance = new Instance();
                        newInstance.header = ReadHeader(reader);

                        newInstance.modelName = ReadStringUInt32(reader).ToLower().Replace(".i3d", ".4ds");

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