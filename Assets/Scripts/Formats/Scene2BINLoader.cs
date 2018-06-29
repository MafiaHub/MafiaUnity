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
                HEADER_MISSION = 0x4c53,
                HEADER_META = 0x0001,
                HEADER_UNK_FILE = 0xAFFF,
                HEADER_UNK_FILE2 = 0x3200,
                HEADER_FOV = 0x3010,
                HEADER_VIEW_DISTANCE = 0x3011,
                HEADER_CLIPPING_PLANES = 0x3211,
                HEADER_WORLD = 0x4000,
                HEADER_SPECIAL_WORLD = 0xAE20,
                HEADER_ENTITIES = 0xAE20,
                HEADER_INIT = 0xAE50,
                // WORLD subHeader
                HEADER_OBJECT = 0x4010,
                HEADER_SPECIAL_OBJECT = 0xAE21,
            }

            [Flags]
            public enum ObjectProperty : uint
            {
                OBJECT_TYPE_SPECIAL = 0xAE22,
                OBJECT_TYPE_NORMAL = 0x4011,
                OBJECT_POSITION = 0x0020,
                OBJECT_ROTATION = 0x0022,
                OBJECT_POSITION_2 = 0x002C,
                OBJECT_SCALE = 0x002D,
                OBJECT_PARENT = 0x4020,
                OBJECT_NAME = 0x0010,
                OBJECT_NAME_SPECIAL = 0xAE23,
                OBJECT_MODEL = 0x2012,
                OBJECT_LIGHT_MAIN = 0x4040,
                OBJECT_LIGHT_TYPE = 0x4041,
                OBJECT_LIGHT_COLOUR = 0x0026,
                OBJECT_LIGHT_POWER = 0x4042,
                OBJECT_LIGHT_UNK_1 = 0x4043,
                OBJECT_LIGHT_RANGE = 0x4044,
                OBJECT_LIGHT_FLAGS = 0x4045,
                OBJECT_LIGHT_SECTOR = 0x4046,
                OBJECT_SPECIAL_DATA = 0xAE24,
            }

            [Flags]
            public enum ObjectType : uint
            {
                OBJECT_TYPE_LIGHT = 0x02,
                OBJECT_TYPE_CAMERA = 0x03,
                OBJECT_TYPE_SOUND = 0x04,
                OBJECT_TYPE_MODEL = 0x09,
                OBJECT_TYPE_OCCLUDER = 0x0C,
                OBJECT_TYPE_SECTOR = 0x99,
                OBJECT_TYPE_SCRIPT = 0x9B
            }

            [Flags]
            public enum SpecialObjectType : uint
            {
                SPECIAL_OBJECT_TYPE_NONE = 0,
                SPECIAL_OBJECT_TYPE_PHYSICAL = 0x23,
                SPECIAL_OBJECT_TYPE_PLAYER = 0x02,
                SPECIAL_OBJECT_TYPE_CHARACTER = 0x1B,
                SPECIAL_OBJECT_TYPE_CAR = 0x06,
                SPECIAL_OBJECT_TYPE_PUB_VEHICLE = 0x08,
                SPECIAL_OBJECT_TYPE_SCRIPT = 0x05,
            }

            [Flags]
            public enum LightType : uint
            {
                LIGHT_TYPE_POINT = 0x01,
                LIGHT_TYPE_DIRECTIONAL = 0x03,
                LIGHT_TYPE_AMBIENT = 0x04,
                LIGHT_TYPE_FOG = 0x05,
                LIGHT_TYPE_POINT_AMBIENT = 0x06,
                LIGHT_TYPE_LAYERED_FOG = 0x08,
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
                    case HeaderType.HEADER_SPECIAL_WORLD:
                    case HeaderType.HEADER_WORLD:
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

                    case HeaderType.HEADER_VIEW_DISTANCE:
                    {
                        viewDistance = reader.ReadSingle();
                    }
                    break;

                    case HeaderType.HEADER_CLIPPING_PLANES:
                    {
                        clippingPlanes = ReadVector2(reader);
                    }
                    break;

                    case HeaderType.HEADER_FOV:
                    {
                        FOV = reader.ReadSingle();
                    }
                    break;

                    case HeaderType.HEADER_SPECIAL_OBJECT:
                    case HeaderType.HEADER_OBJECT:
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
                            if (header.type == HeaderType.HEADER_OBJECT && !objects.ContainsKey(newObject.name))
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
                    case ObjectProperty.OBJECT_TYPE_SPECIAL:
                    {
                        newObject.specialType = (SpecialObjectType)reader.ReadUInt32();
                    }
                    break;

                    case ObjectProperty.OBJECT_TYPE_NORMAL:
                    {
                        newObject.type = (ObjectType)reader.ReadUInt32();
                    }
                    break;

                    case ObjectProperty.OBJECT_NAME:
                    case ObjectProperty.OBJECT_NAME_SPECIAL:
                    {
                            var charName = reader.ReadBytes((int)header.size-7);
                            
                            newObject.name = System.Text.Encoding.ASCII.GetString(charName);
                    }
                    break;

                    case ObjectProperty.OBJECT_SPECIAL_DATA:
                        {
                            switch (newObject.specialType)
                            {
                                case SpecialObjectType.SPECIAL_OBJECT_TYPE_PHYSICAL:
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

                    case ObjectProperty.OBJECT_MODEL:
                        {
                            var charName = reader.ReadBytes((int)header.size - 6);
                            
                            newObject.modelName = System.Text.Encoding.ASCII.GetString(charName).Replace(".I3D", ".4ds");
                            newObject.modelName = newObject.modelName.Replace(".i3d", ".4ds");
                            newObject.modelName = newObject.modelName.Trim().Split('\0')[0];
                        }
                        break;

                    case ObjectProperty.OBJECT_POSITION:
                        {
                            newObject.pos = ReadVector3(reader);
                        }
                        break;

                    case ObjectProperty.OBJECT_ROTATION:
                        {
                            var rot = ReadQuat(reader);
                            newObject.rot = new Quaternion(rot.y, rot.z, rot.w, -1 * rot.x);
                        }
                        break;

                    case ObjectProperty.OBJECT_POSITION_2:
                        {
                            newObject.pos2 = ReadVector3(reader);
                        }
                        break;

                    case ObjectProperty.OBJECT_SCALE:
                        {
                            newObject.scale = ReadVector3(reader);
                        }
                        break;

                    case ObjectProperty.OBJECT_LIGHT_MAIN:
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

                    case ObjectProperty.OBJECT_PARENT:
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
                    case ObjectProperty.OBJECT_LIGHT_TYPE:
                        {
                            newObject.lightType = (LightType)reader.ReadInt32();
                        }
                        break;

                    case ObjectProperty.OBJECT_LIGHT_COLOUR:
                        {
                            newObject.lightColour = ReadVector3(reader);
                        }
                        break;

                    case ObjectProperty.OBJECT_LIGHT_POWER:
                        {
                            newObject.lightPower = reader.ReadSingle();
                        }
                        break;

                    case ObjectProperty.OBJECT_LIGHT_RANGE:
                        {
                            newObject.lightNear = reader.ReadSingle();
                            newObject.lightFar = reader.ReadSingle();
                        }
                        break;

                    case ObjectProperty.OBJECT_LIGHT_SECTOR:
                        {
                            //read(srcFile, object->mLightSectors, header->mSize);
                        }
                        break;

                    case ObjectProperty.OBJECT_LIGHT_FLAGS:
                        {
                            newObject.lightFlags = reader.ReadInt32();
                        }
                        break;

                    case ObjectProperty.OBJECT_LIGHT_UNK_1:
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