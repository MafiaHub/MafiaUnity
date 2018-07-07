using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenMafia
{
    namespace MafiaFormats
    {
        public class Scene2BINLoader : BaseLoader
        {
            public float viewDistance;
            public float FOV;
            public Vector2 clippingPlanes;

            [Flags]
            public enum HeaderType : uint
            {
                Mission = 0x4c53,
                Meta = 0x0001,
                Unknown_File = 0xAFFF,
                Unknown_File2 = 0x3200,
                Fov = 0x3010,
                ViewDistance = 0x3011,
                ClippingPlanes = 0x3211,
                World = 0x4000,
                SpecialWorld = 0xAE20,
                Entities = 0xAE20,
                Init = 0xAE50,
                // WORLD subHeader
                Object = 0x4010,
                SpecialObject = 0xAE21,
            }

            [Flags]
            public enum ObjectProperty : uint
            {
                TypeSpecial = 0xAE22,
                TypeNormal = 0x4011,
                Position = 0x0020,
                Rotation = 0x0022,
                Position2 = 0x002C,
                Scale = 0x002D,
                Parent = 0x4020,
                Name = 0x0010,
                Name_Special = 0xAE23,
                Model = 0x2012,
                Light_Main = 0x4040,
                Light_Type = 0x4041,
                Light_Color = 0x0026,
                Light_Power = 0x4042,
                Light_Unknown = 0x4043,
                Light_Range = 0x4044,
                Light_Flags = 0x4045,
                Light_Sector = 0x4046,
                SpecialData = 0xAE24,
            }

            [Flags]
            public enum ObjectType : uint
            {
                Light = 0x02,
                Camera = 0x03,
                Sound = 0x04,
                Model = 0x09,
                Occluder = 0x0C,
                Sector = 0x99,
                Script = 0x9B
            }

            [Flags]
            public enum SpecialObjectType : uint
            {
                None = 0,
                Physical = 0x23,
                Player = 0x02,
                Character = 0x1B,
                Car = 0x06,
                Public_Vehicle = 0x08,
                Script = 0x05,
            }

            [Flags]
            public enum LightType : uint
            {
                Point = 0x01,
                Directional = 0x03,
                Ambient = 0x04,
                Fog = 0x05,
                Point_Ambient = 0x06,
                Layered_Fog = 0x08,
            }

            public struct Header
            {
                public HeaderType type;
                public uint size;
            }

            public struct SpecialProp
            {
                // Physical object properties
                public float movVal1;
                public float movVal2;
                public float friction;
                public float movVal4;
                public int movVal5;
                public float weight;
                public int sound;
            }

            public struct Object
            {
                public ObjectType type;
                public SpecialObjectType specialType;
                public Vector3 pos;
                public Quaternion rot;
                public Vector3 pos2; // precomputed final world transform position
                public Vector3 scale;
                public string name;
                public string modelName;
                public string parentName;

                // Light properties
                public LightType lightType;
                public Vector3 lightColour;
                public int lightFlags;
                public float lightPower;           // 1.0 = 100% (can be even over 1.0)
                public float lightUnk0;
                public float lightUnk1;
                public float lightNear;
                public float lightFar;
                public char[] lightSectors; //5000
                public SpecialProp physicalObject;
            }

            public Dictionary<string, Object> objects = new Dictionary<string, Object>();

            private Header ReadHeader(BinaryReader reader)
            {
                Header newHeader = new Header();
                newHeader.type = (HeaderType)reader.ReadUInt16();
                newHeader.size = reader.ReadUInt32();
                return newHeader;
            }

            public void Load(BinaryReader reader)
            {
                var header = ReadHeader(reader);
                uint position = 6;

                while (position + 6 < header.size)
                {
                    reader.BaseStream.Seek(position, SeekOrigin.Begin);
                    var nextHeader = ReadHeader(reader);
                    ReadHeader(reader, ref nextHeader, position + 6);

                    position += nextHeader.size;
                }
            }

            private void ReadHeader(BinaryReader reader, ref Header header, uint offset)
            {
                switch (header.type)
                {
                    case HeaderType.SpecialWorld:
                    case HeaderType.World:
                    {
                        uint position = offset;
                        while (position + 6 < offset + header.size)
                        {
                            reader.BaseStream.Seek(position, SeekOrigin.Begin);
                            var nextHeader = ReadHeader(reader);
                            ReadHeader(reader, ref nextHeader, position + 6);
                            position += nextHeader.size;
                        }
                    }
                    break;

                    case HeaderType.ViewDistance:
                    {
                        viewDistance = reader.ReadSingle();
                    }
                    break;

                    case HeaderType.ClippingPlanes:
                    {
                        clippingPlanes = ReadVector2(reader);
                    }
                    break;

                    case HeaderType.Fov:
                    {
                        FOV = reader.ReadSingle();
                    }
                    break;

                    case HeaderType.SpecialObject:
                    case HeaderType.Object:
                        {
                            uint position = offset;
                            Object newObject = new Object();

                            while (position + 6 < offset + header.size)
                            {
                                reader.BaseStream.Seek(position, SeekOrigin.Begin);
                                var nextHeader = ReadHeader(reader);
                                ReadObject(reader, ref nextHeader, ref newObject, position + 6);
                                position += nextHeader.size;
                            }

                            //TODO SEction 
                            if (header.type == HeaderType.Object && !objects.ContainsKey(newObject.name))
                            {
                                objects.Add(newObject.name, newObject);
                            }
                            else
                            {
                                if (objects.ContainsKey(newObject.name))
                                {
                                    var targetObject = objects[newObject.name];
                                    targetObject.specialType = newObject.specialType;
                                    targetObject.physicalObject = newObject.physicalObject;
                                }
                            }
                        }
                        break;
                }
            }
            
            private void ReadObject(BinaryReader reader, ref Header header, ref Object newObject, uint offset)
            {
                switch ((ObjectProperty)header.type)
                {
                    case ObjectProperty.TypeSpecial:
                    {
                        newObject.specialType = (SpecialObjectType)reader.ReadUInt32();
                    }
                    break;

                    case ObjectProperty.TypeNormal:
                    {
                        newObject.type = (ObjectType)reader.ReadUInt32();
                    }
                    break;

                    case ObjectProperty.Name:
                    case ObjectProperty.Name_Special:
                    {
                            var charName = reader.ReadBytes((int)header.size-7);
                            
                            newObject.name = System.Text.Encoding.ASCII.GetString(charName);
                    }
                    break;

                    case ObjectProperty.SpecialData:
                        {
                            switch (newObject.specialType)
                            {
                                case SpecialObjectType.Physical:
                                    {
                                        reader.BaseStream.Seek(2, SeekOrigin.Current);
                                        var newSpecialObject = new SpecialProp();
                                        newSpecialObject.movVal1 = reader.ReadSingle();
                                        newSpecialObject.movVal2 = reader.ReadSingle();
                                        newSpecialObject.weight = reader.ReadSingle();
                                        newSpecialObject.friction = reader.ReadSingle();
                                        newSpecialObject.movVal4 = reader.ReadSingle();
                                        newSpecialObject.sound = reader.ReadInt32();
                                        reader.BaseStream.Seek(1, SeekOrigin.Current);
                                        newSpecialObject.movVal5 = reader.ReadInt32();
                                        newObject.physicalObject = newSpecialObject;
                                    }
                                    break;
                            }
                        }
                        break;

                    case ObjectProperty.Model:
                        {
                            var charName = reader.ReadBytes((int)header.size - 6);
                            
                            newObject.modelName = System.Text.Encoding.ASCII.GetString(charName).Replace(".I3D", ".4ds");
                            newObject.modelName = newObject.modelName.Replace(".i3d", ".4ds");
                            newObject.modelName = newObject.modelName.Trim().Split('\0')[0];
                        }
                        break;

                    case ObjectProperty.Position:
                        {
                            newObject.pos = ReadVector3(reader);
                        }
                        break;

                    case ObjectProperty.Rotation:
                        {
                            var rot = ReadQuat(reader);
                            newObject.rot = new Quaternion(rot.y, rot.z, rot.w, -1 * rot.x);
                        }
                        break;

                    case ObjectProperty.Position2:
                        {
                            newObject.pos2 = ReadVector3(reader);
                        }
                        break;

                    case ObjectProperty.Scale:
                        {
                            newObject.scale = ReadVector3(reader);
                        }
                        break;

                    case ObjectProperty.Light_Main:
                        {
                            uint position = offset;
                            while (position + 6 < offset + header.size)
                            {
                                var lightHeader = ReadHeader(reader);
                                ReadLight(reader, ref lightHeader, ref newObject);
                                position += lightHeader.size;
                            }
                        }
                        break;

                    case ObjectProperty.Parent:
                        {
                            var parentHeader = ReadHeader(reader);
                            Object parentObject = new Object();
                            ReadObject(reader, ref parentHeader, ref parentObject, offset + 6);
                            newObject.parentName = parentObject.name;
                        }
                        break;
                }
            }

            private void ReadLight(BinaryReader reader, ref Header header, ref Object newObject)
            {
                switch ((ObjectProperty)header.type)
                {
                    case ObjectProperty.Light_Type:
                        {
                            newObject.lightType = (LightType)reader.ReadInt32();
                        }
                        break;

                    case ObjectProperty.Light_Color:
                        {
                            newObject.lightColour = ReadVector3(reader);
                        }
                        break;

                    case ObjectProperty.Light_Power:
                        {
                            newObject.lightPower = reader.ReadSingle();
                        }
                        break;

                    case ObjectProperty.Light_Range:
                        {
                            newObject.lightNear = reader.ReadSingle();
                            newObject.lightFar = reader.ReadSingle();
                        }
                        break;

                    case ObjectProperty.Light_Sector:
                        {
                            //read(srcFile, object->mLightSectors, header->mSize);
                        }
                        break;

                    case ObjectProperty.Light_Flags:
                        {
                            newObject.lightFlags = reader.ReadInt32();
                        }
                        break;

                    case ObjectProperty.Light_Unknown:
                        {
                            newObject.lightUnk0 = reader.ReadSingle();
                            newObject.lightUnk1 = reader.ReadSingle();
                        }
                        break;
                }
            }
        }
    }
}