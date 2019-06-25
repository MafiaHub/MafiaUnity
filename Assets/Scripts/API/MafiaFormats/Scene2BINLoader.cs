using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MafiaFormats
{
    public partial class Scene2BINLoader : BaseLoader
    {
        public float viewDistance;
        public float FOV;
        public Vector2 clippingPlanes;

        [Flags]
        [Serializable]
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
        [Serializable]
        public enum ObjectProperty : uint
        {
            TypeSpecial = 0xAE22,
            TypeNormal = 0x4011,
            Position = 0x0020,
            Rotation = 0x0022,
            Position2 = 0x002C,
            Scale = 0x002D,
            Parent = 0x4020,
            Hidden = 0x4033,
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

        [Serializable]
        public enum ObjectType : uint
        {
            None = 0x00,
            Light = 0x02,
            Camera = 0x03,
            Sound = 0x04,
            Model = 0x09,
            Occluder = 0x0C,
            Sector = 0x99,
            Lightmap = 0x9A,
            Script = 0x9B,
            LastType
        }

        [Serializable]
        public enum SpecialObjectType : uint
        {
            None = 0x00,
            Physical = 0x23,
            Player = 0x02,
            Character = 0x1B,
            Car = 0x04,
            Door = 0x06,
            Dog = 0x15,
            Pumper = 0x19,
            Public_Vehicle = 0x08,
            Script = 0x05,
        }

        [Flags]
        [Serializable]
        public enum LightType : int
        {
            Point = 0x01,
            Spot = 0x02,
            Directional = 0x03,
            Ambient = 0x04,
            Fog = 0x05,
            Point_Ambient = 0x06,
            Layered_Fog = 0x08,
        }

        [Flags]
        public enum LightFlags : int
        {
            DynamicShadows = (1 << 3),
            LightmapShadows = (1 << 5)
        }

        public class Header
        {
            public HeaderType type;
            public uint size;
        }

        [Serializable]
        public class PhysicalProp
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

        [Serializable]
        public class DoorProp
        {
            // Door properties
            public byte open1;
            public byte open2;
            public float moveAngle;
            public byte open;
            public byte locked;
            public float closeSpeed;
            public float openSpeed;
            public string openSound;
            public string closeSound;
            public string lockedSound;
            public byte flag;
        }

        [Serializable]
        public class Object
        {
            public bool isPatch=true;
            public bool isHidden=false;
            public ObjectType type;
            public SpecialObjectType specialType;
            public Vector3 pos;
            public bool isPositionPatched = false;
            public Quaternion rot;
            public bool isRotationPatched = false;
            public Vector3 pos2; // precomputed final world transform position
            public bool isPosition2Patched = false;
            public Vector3 scale;
            public bool isScalePatched = false;
            public string name;
            public string modelName;
            public string parentName;
            public bool isParentPatched = false;

            // Light properties
            public LightType lightType;
            public Vector3 lightColour;
            public LightFlags lightFlags;
            public float lightPower;           // 1.0 = 100% (can be even over 1.0)
            public float lightUnk0;
            public float lightAngle;
            public float lightNear;
            public float lightFar;
            public string lightSectors; //5000
            public PhysicalProp physicalObject;
            public DoorProp doorObject;
        }

        public Dictionary<string, Object> objects = new Dictionary<string, Object>();

        // These objects come from a different source and have to be handled separately
        public Dictionary<string, Object> externalObjects = new Dictionary<string, Object>();

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
 
                    // Check for the type
                    if (header.type == HeaderType.Object)
                    {
                        objects.Add(newObject.name, newObject);
                    }
                    // This object redefines an existing object
                    else
                    {
                        // Extra data is provided to an existing object
                        if (objects.ContainsKey(newObject.name))
                        {
                            var targetObject = objects[newObject.name];
                            targetObject.specialType = newObject.specialType;
                            targetObject.physicalObject = newObject.physicalObject;
                            targetObject.doorObject = newObject.doorObject;
                        }
                        // Object provides data to an external source (scene.4ds, ...)
                        else
                        {
                            externalObjects.Add(newObject.name, newObject);
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

                    if (newObject.type > 0 && newObject.type < ObjectType.LastType)
                        newObject.isPatch = false;
                }
                break;

                case ObjectProperty.Name:
                case ObjectProperty.Name_Special:
                {
                        newObject.name = ReadTerminatedString(reader);
                }
                break;

                case ObjectProperty.SpecialData:
                {
                    switch (newObject.specialType)
                    {
                        case SpecialObjectType.Physical:
                        {
                            reader.BaseStream.Seek(2, SeekOrigin.Current);
                            var newSpecialObject = new PhysicalProp();
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

                        case SpecialObjectType.Door:
                        {
                            reader.BaseStream.Seek(5, SeekOrigin.Current);
                            var newSpecialObject = new DoorProp();
                            newSpecialObject.open1 = reader.ReadByte();
                            newSpecialObject.open2 = reader.ReadByte();
                            newSpecialObject.moveAngle = reader.ReadSingle();
                            newSpecialObject.open = reader.ReadByte();
                            newSpecialObject.locked = reader.ReadByte();
                            newSpecialObject.closeSpeed = reader.ReadSingle();
                            newSpecialObject.openSpeed = reader.ReadSingle();
                            newSpecialObject.openSound = ReadTerminatedString(reader);
                            reader.BaseStream.Seek((16 - newSpecialObject.openSound.Length - 1), SeekOrigin.Current);
                            newSpecialObject.closeSound = ReadTerminatedString(reader);
                            reader.BaseStream.Seek((16 - newSpecialObject.closeSound.Length - 1), SeekOrigin.Current);
                            newSpecialObject.lockedSound = ReadTerminatedString(reader);
                            reader.BaseStream.Seek((16 - newSpecialObject.lockedSound.Length - 1), SeekOrigin.Current);
                            newSpecialObject.flag = reader.ReadByte();
                            newObject.doorObject = newSpecialObject;
                        }
                        break;
                    }
                }
                break;

                case ObjectProperty.Model:
                {
                    newObject.modelName = ReadTerminatedString(reader).ToLower().Replace(".i3d", ".4ds");
                }
                break;

                case ObjectProperty.Hidden:
                {
                    newObject.isHidden = true;
                }
                break;

                case ObjectProperty.Position:
                {
                    newObject.pos = ReadVector3(reader);
                    newObject.isPositionPatched = true;
                }
                break;

                case ObjectProperty.Rotation:
                {
                    newObject.rot = ReadQuat(reader);
                    newObject.isRotationPatched = true;
                }
                break;

                case ObjectProperty.Position2:
                {
                    newObject.pos2 = ReadVector3(reader);
                    newObject.isPosition2Patched = true;
                }
                break;

                case ObjectProperty.Scale:
                {
                    newObject.scale = ReadVector3(reader);
                    newObject.isScalePatched = true;
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
                    newObject.isParentPatched = true;
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
                    newObject.lightSectors = ReadTerminatedString(reader);
                }
                break;

                case ObjectProperty.Light_Flags:
                {
                    newObject.lightFlags = (LightFlags)reader.ReadInt32();
                }
                break;

                case ObjectProperty.Light_Unknown:
                {
                    newObject.lightUnk0 = reader.ReadSingle();
                    newObject.lightAngle = reader.ReadSingle();
                }
                break;
            }
        }
    }
}