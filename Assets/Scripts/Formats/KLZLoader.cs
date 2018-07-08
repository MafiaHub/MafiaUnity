using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace MafiaUnity
{
    public class KLZLoader : BaseLoader
    {
        [Flags]
        public enum GridReference : uint
        {
            REFERENCE_FACE = 0x00,
            REFERENCE_XTOBB = 0x80,
            REFERENCE_AABB = 0x81,
            REFERENCE_SPHERE = 0x82,
            REFERENCE_OBB = 0x83,
            REFERENCE_CYLINDER = 0x84
        }

        public struct Header
        {
            public uint signature;
            public uint version;
            public uint collisionDataOffset;
            public uint numLinks;
            public uint unknown1;
            public uint unknown2;
        }

        public struct Link
        {
            public uint flags;
            public string name;
        }

        public struct DataHeader
        {
            public float gridMinX;
            public float gridMinY;
            public float gridMaxX;
            public float gridMaxY;
            public float cellWidth;
            public float cellHeight;
            public uint gridWidth;
            public uint gridHeight;
            public uint unknown0;
            public uint reserved_00;
            public uint reserved_01;
            public uint reserved1;
            public uint numFaces;
            public uint reserved2;
            public uint numXTOBBs;
            public uint reserved3;
            public uint numAABBs;
            public uint reserved4;
            public uint numSpheres;
            public uint reserved5;
            public uint numOBBs;
            public uint reserved6;
            public uint numCylinders;
            public uint reserved7;
            public uint numUnknownType;  // always 0
            public uint unknown1;
        }

        public struct Properties
        {
            public byte material;
            public byte flags;
            public byte sortInfo;   // only used with FaceCol
            public byte unknown;    // different values for different collision types of objects
        }
    
        public struct FaceVertexIndex
        {
            public ushort index;
            public ushort link;            // index to link table, this will be the same for all three indices of one face
        }  
;
        
        public struct FaceCol
        {
            public Properties properties;        // NOTE(ASM): Material (8 bit) | Flags (8 bit) | SortInfo (8 bit) | 0 (8 bit)
            public FaceVertexIndex[] indices; // [3]
            public Vector3 normal;                  // NOTE(ASM): needs to point in opposite direction compared to the mesh face normal (IIRC!), i.e. if the mesh face normal is (1 0 0), the col face normal needs to be (-1 0 0)
            public float distance;
        }

        public struct AABBCol
        {
            public Properties properties;   // NOTE(ASM): Material(8 bit) | Flags (8 bit) | 0 (8 bit) | 0x81 (8 bit)
            public uint link;         // NOTE(ASM): index into LinkNameOffsetTable
            public Vector3 min;              // first point that defines the box in space
            public Vector3 max;              // second point that defines the box in space
        }                   // axis-aligned bounding box

        public struct XTOBBCol
        {
            public Properties properties;   // NOTE(ASM): Material(8 bit) | Flags (8 bit) | 0 (8 bit) | 0x80 (8 bit)
            public uint link;
            public Vector3 min;              // precomputed AABB
            public Vector3 max;
            public Vector3[] extends;       //[2] BB corners to be transformed
            public Matrix4x4 transform;
            public Matrix4x4 inverseTransform;
        }                 // oriented bounding box, in addition to OBB has an additional precomputed AABB

        public struct CylinderCol
        {
            public Properties properties;    // NOTE(ASM): Material(8 bit) | Flags (8 bit) | 0 (8 bit) | 0x84 (8 bit)
            public uint link;
            public Vector2 position;         // NOTE(ASM): cylinders only have a 2d position!
            public float radius;
        }              // cylindrical collision object
  
        public struct OBBCol
        {
            public Properties properties;   // NOTE(ASM): Material(8 bit) | Flags (8 bit) | 0 (8 bit) | 0x83 (8 bit)
            public uint link;
            public Vector3[] extends;       //[2] two box corners, however the box seems to be symmetrical around [0,0,0], so one is redundant
            public Matrix4x4 transform;
            public Matrix4x4 inverseTransform;
        }                   // oriented bounding box

        public struct SphereCol
        {
            public Properties properties;   // NOTE(ASM): Material(8 bit) | Flags (8 bit) | 0 (8 bit) | 0x82 (8 bit)
            public uint link;
            public Vector3 position;
            public float radius;
        }                // spherical collision object

        public struct Cell
        {
            public uint numObjects;
            public uint[] reserved; // [2]
            public float height;
            public uint references;  //REF! NOTE(ASM): (Type (8 bit)) | (Offset into array of Type (24 bit)))
            public char flags; //REF!
        }

        public List<Link> linkTables            = new List<Link>();
        public List<FaceCol> faceCols           = new List<FaceCol>();
        public List<AABBCol> AABBCols           = new List<AABBCol>();
        public List<XTOBBCol> XTOBBCols         = new List<XTOBBCol>();
        public List<CylinderCol> cylinderCols   = new List<CylinderCol>();
        public List<OBBCol> OBBCols             = new List<OBBCol>();
        public List<SphereCol> sphereCols       = new List<SphereCol>();

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

        public Header ReadHeader(BinaryReader reader)
        {
            Header newHeader = new Header();
            newHeader.signature = reader.ReadUInt32();
            newHeader.version = reader.ReadUInt32();
            newHeader.collisionDataOffset = reader.ReadUInt32();
            newHeader.numLinks = reader.ReadUInt32();
            newHeader.unknown1 = reader.ReadUInt32();
            newHeader.unknown2 = reader.ReadUInt32();
            return newHeader;
        }

        public DataHeader ReadDataHeader(BinaryReader reader)
        {
            DataHeader newDataHeader = new DataHeader();
            newDataHeader.gridMinX = reader.ReadSingle();
            newDataHeader.gridMinY = reader.ReadSingle();
            newDataHeader.gridMaxX = reader.ReadSingle();
            newDataHeader.gridMaxY = reader.ReadSingle();
            newDataHeader.cellWidth = reader.ReadSingle();
            newDataHeader.cellHeight = reader.ReadSingle();
            newDataHeader.gridWidth = reader.ReadUInt32();
            newDataHeader.gridHeight = reader.ReadUInt32();
            newDataHeader.unknown0 = reader.ReadUInt32();
            newDataHeader.reserved_00 = reader.ReadUInt32();
            newDataHeader.reserved_01 = reader.ReadUInt32();
            newDataHeader.reserved1 = reader.ReadUInt32();
            newDataHeader.numFaces = reader.ReadUInt32();
            newDataHeader.reserved2 = reader.ReadUInt32();
            newDataHeader.numXTOBBs = reader.ReadUInt32();
            newDataHeader.reserved3 = reader.ReadUInt32();
            newDataHeader.numAABBs = reader.ReadUInt32();
            newDataHeader.reserved4 = reader.ReadUInt32();
            newDataHeader.numSpheres = reader.ReadUInt32();
            newDataHeader.reserved5 = reader.ReadUInt32();
            newDataHeader.numOBBs = reader.ReadUInt32();
            newDataHeader.reserved6 = reader.ReadUInt32();
            newDataHeader.numCylinders = reader.ReadUInt32();
            newDataHeader.reserved7 = reader.ReadUInt32();
            newDataHeader.numUnknownType = reader.ReadUInt32();
            newDataHeader.unknown1 = reader.ReadUInt32();
            return newDataHeader;
        }

        public Properties ReadProperties(BinaryReader reader)
        {
            Properties newProperties = new Properties();
            newProperties.material = reader.ReadByte();
            newProperties.flags = reader.ReadByte();
            newProperties.sortInfo = reader.ReadByte();
            newProperties.unknown = reader.ReadByte();

            return newProperties;
        }

        public FaceVertexIndex ReadVertexIndex(BinaryReader reader)
        {
            FaceVertexIndex newVertexIndex = new FaceVertexIndex();
            newVertexIndex.index = reader.ReadUInt16();
            newVertexIndex.link = reader.ReadUInt16();
            return newVertexIndex;
        }

        public FaceCol ReadFaceCol(BinaryReader reader)
        {
            FaceCol newFaceCol = new FaceCol();
            newFaceCol.properties = ReadProperties(reader);

            newFaceCol.indices = new FaceVertexIndex[3];
            for (var i = 0; i < 3; i++)
                newFaceCol.indices[i] = ReadVertexIndex(reader);

            newFaceCol.normal = ReadVector3(reader);
            newFaceCol.distance = reader.ReadSingle();
            return newFaceCol;
        }

        public AABBCol ReadAABBCol(BinaryReader reader)
        {
            AABBCol newAABBCol = new AABBCol();
            newAABBCol.properties = ReadProperties(reader);
            newAABBCol.link = reader.ReadUInt32();
            newAABBCol.min = ReadVector3(reader);
            newAABBCol.max = ReadVector3(reader);
            return newAABBCol;
        }

        public XTOBBCol ReadXTOBBCol(BinaryReader reader)
        {
            XTOBBCol newXTOBBCol = new XTOBBCol();
            newXTOBBCol.properties = ReadProperties(reader);
            newXTOBBCol.link = reader.ReadUInt32();
            newXTOBBCol.min = ReadVector3(reader);
            newXTOBBCol.max = ReadVector3(reader);

            newXTOBBCol.extends = new Vector3[2];
            for (var i = 0; i < 2; i++)
                newXTOBBCol.extends[i] = ReadVector3(reader);

            newXTOBBCol.transform = ReadMatrix(reader);
            newXTOBBCol.inverseTransform = ReadMatrix(reader);

            return newXTOBBCol;
        }

        public CylinderCol ReadCylinderCol(BinaryReader reader)
        {
            CylinderCol newCylinderCol = new CylinderCol();
            newCylinderCol.properties = ReadProperties(reader);
            newCylinderCol.link = reader.ReadUInt32();
            newCylinderCol.position = ReadVector2(reader);
            newCylinderCol.radius = reader.ReadSingle();
            return newCylinderCol;
        }

        public OBBCol ReadOBBCol(BinaryReader reader)
        {
            OBBCol newOBBCol = new OBBCol();
            newOBBCol.properties = ReadProperties(reader);
            newOBBCol.link = reader.ReadUInt32();
            newOBBCol.extends = new Vector3[2];

            for (var i = 0; i < 2; i++)
                newOBBCol.extends[i] = ReadVector3(reader);

            newOBBCol.transform = ReadMatrix(reader);
            newOBBCol.inverseTransform = ReadMatrix(reader);
            return newOBBCol;
        }

        public SphereCol ReadSphereCol(BinaryReader reader)
        {
            SphereCol newSpehereCol = new SphereCol();
            newSpehereCol.properties = ReadProperties(reader);
            newSpehereCol.link = reader.ReadUInt32();
            newSpehereCol.position = ReadVector3(reader);
            newSpehereCol.radius = reader.ReadSingle();
            return newSpehereCol;
        }

        public void load(BinaryReader reader)
        {
            var newHeader = ReadHeader(reader);

            uint[] linkNameOffsetTable = new uint[newHeader.numLinks];
            for (var i = 0; i < newHeader.numLinks; i++)
                linkNameOffsetTable[i] = reader.ReadUInt32();

            for (var i = 0; i < newHeader.numLinks; i++)
            {
                Link newLink = new Link();
                reader.BaseStream.Seek(linkNameOffsetTable[i], SeekOrigin.Begin);
                newLink.flags = reader.ReadUInt32();
                var stringLength = peekLength(reader);
                newLink.name = new string(reader.ReadChars(stringLength));
                linkTables.Add(newLink);
            }

            reader.BaseStream.Seek(newHeader.collisionDataOffset, SeekOrigin.Begin);

            var newDataHeader = ReadDataHeader(reader);

            float[] cellBoundariesX = new float[newDataHeader.gridWidth + 1];
            for (var i = 0; i < newDataHeader.gridWidth + 1; i++)
                cellBoundariesX[i] = reader.ReadSingle();

            float[] cellBoundariesY = new float[newDataHeader.gridHeight + 1];
            for(var i = 0; i < newDataHeader.gridHeight + 1; i++)
                cellBoundariesY[i] = reader.ReadSingle();


            uint collisionDataMagic = reader.ReadUInt32();

            for (var i = 0; i < newDataHeader.numFaces; i++)
                faceCols.Add(ReadFaceCol(reader));
               
            for (var i = 0; i < newDataHeader.numAABBs; i++)
                AABBCols.Add(ReadAABBCol(reader));

            for (var i = 0; i < newDataHeader.numXTOBBs; i++)
                XTOBBCols.Add(ReadXTOBBCol(reader));

            for (var i = 0; i < newDataHeader.numCylinders; i++)
                cylinderCols.Add(ReadCylinderCol(reader));

            for (var i = 0; i < newDataHeader.numOBBs; i++)
                OBBCols.Add(ReadOBBCol(reader));

            for (var i = 0; i < newDataHeader.numSpheres; i++)
                sphereCols.Add(ReadSphereCol(reader));

            //TODO(DavoSK): GridLoading .. not needed for now
        }
    }
}
