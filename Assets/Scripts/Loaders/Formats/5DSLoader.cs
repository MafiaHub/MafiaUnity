using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace MafiaUnity
{ 
    namespace MafiaFormats
    {
        public class Loader5DS : BaseLoader 
        {
            [Flags]
            public enum TypeOfSequence : uint
            {
                Position = 0x2,
                Rotation = 0x4,
                Scale = 0x8,
                Note = 0x20
            }

            [Flags]
            public enum FileVersion : ushort
            {
                Mafia = 20,
                HD2 = 122,
                Chameleon = 123,
            }
            
            public class Header
            {
                // should be "5DS\0" 
                public uint magicByte;
                // should be 0x14
                public FileVersion fileVersion;
                public uint unk1; //FILETIME DWORD1
                public uint unk2; //FILETIME DWORD2
                public uint lengthOfAnimationData;
            }

            public class Description
            {
                public ushort numberOfAnimatedObjects;
                // Note: 25 frames = 1 seconds
                public ushort mOverallCountOfFrames;
            }

            public class PointerTable
            {
                public uint pointerToString;
                public uint pointerToData;
            }
            
            [Serializable]
            public class AnimationSequence
            {
                public string objectName;
                public List<uint> rotationFrames    = new List<uint>();
                public List<uint> positionFrames   = new List<uint>();
                public List<uint> scaleFrames      = new List<uint>();

                public List<Vector3> positions      = new List<Vector3>();
                public List<Quaternion> rotations   = new List<Quaternion>();
                public List<Vector3> scales         = new List<Vector3>();

                public ushort numberOfSequences;
                public TypeOfSequence type;

                public bool hasMovement()
                {
                    return type.HasFlag(TypeOfSequence.Position);
                }

                public bool hasRotation()
                {
                    return type.HasFlag(TypeOfSequence.Rotation);
                }

                public bool hasScale()
                {
                    return type.HasFlag(TypeOfSequence.Scale);
                }
            }

            public List<AnimationSequence> sequences = new List<AnimationSequence>();
            public uint totalFrameCount;

            //TODO(DavoSK): Move to utils
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

            public void parseAnimationSequence(ref BinaryReader reader, uint pointerToData, uint pointerToString)
            {
                AnimationSequence newSequence = new AnimationSequence();
                reader.BaseStream.Seek(pointerToData, SeekOrigin.Begin);

                //read block type
                newSequence.type = (TypeOfSequence)reader.ReadUInt32();
                
                if(newSequence.type.HasFlag(TypeOfSequence.Rotation))
                {
                    var numRotationKeys = reader.ReadUInt16();

                    for (var i = 0; i < numRotationKeys; i++)
                        newSequence.rotationFrames.Add(reader.ReadUInt16());

                    if (numRotationKeys % 2 == 0)
                        reader.BaseStream.Seek(2, SeekOrigin.Current);

                    for (var i = 0; i < numRotationKeys; i++)
                        newSequence.rotations.Add(ReadQuat(reader));
                }

                if (newSequence.type.HasFlag(TypeOfSequence.Position))
                {
                    var numPositionKeys = reader.ReadUInt16();

                    for (var i = 0; i < numPositionKeys; i++)
                        newSequence.positionFrames.Add(reader.ReadUInt16());

                    if (numPositionKeys % 2 == 0)
                        reader.BaseStream.Seek(2, SeekOrigin.Current);

                    for (var i = 0; i < numPositionKeys; i++)
                        newSequence.positions.Add(ReadVector3(reader));
                }

                if (newSequence.type.HasFlag(TypeOfSequence.Scale))
                {
                    var numScaleKeys = reader.ReadUInt16();

                    for (var i = 0; i < numScaleKeys; i++)
                        newSequence.scaleFrames.Add(reader.ReadUInt16());

                    if (numScaleKeys % 2 == 0)
                        reader.BaseStream.Seek(2, SeekOrigin.Current);

                    for (var i = 0; i < numScaleKeys; i++)
                        newSequence.scales.Add(ReadVector3(reader));
                }

                reader.BaseStream.Seek(pointerToString, SeekOrigin.Begin);
                var objNameLenght = peekLength(reader);
                newSequence.objectName = new string(reader.ReadChars(objNameLenght));
                sequences.Add(newSequence);
            }

            public void load(BinaryReader reader)
            {
                Header newHeader = new Header();
                newHeader.magicByte = reader.ReadUInt32();
                newHeader.fileVersion = (FileVersion)reader.ReadUInt16();

                newHeader.unk1 = reader.ReadUInt32();
                newHeader.unk2 = reader.ReadUInt32();
                newHeader.lengthOfAnimationData = reader.ReadUInt32();

                if (newHeader.magicByte != 0x00534435)
                {
                    Debug.LogError("Unable to parse 5DS File wrong header!");
                    return;
                }

                var beginingOfData = reader.BaseStream.Position;

                Description newDescription = new Description();
                newDescription.numberOfAnimatedObjects = reader.ReadUInt16();
                newDescription.mOverallCountOfFrames = reader.ReadUInt16();


                totalFrameCount = newDescription.mOverallCountOfFrames;

                PointerTable newPointerTable = new PointerTable();
                
                for (var i = 0; i < newDescription.numberOfAnimatedObjects; i++)
                {
                    newPointerTable.pointerToString = reader.ReadUInt32();
                    newPointerTable.pointerToData = reader.ReadUInt32();

                    var nextBlock = reader.BaseStream.Position;
                    var pointerToName = (uint)beginingOfData + newPointerTable.pointerToString;
                    var pointerToData = (uint)beginingOfData + newPointerTable.pointerToData;

                    parseAnimationSequence(ref reader, pointerToData, pointerToName);
                    reader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);
                }
            }
        }
    }
}
