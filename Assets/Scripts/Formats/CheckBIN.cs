using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace MafiaUnity
{
    public class CheckBIN : BaseLoader
    {
        [Flags]
        public enum PointType : uint
        {
            Pedestrian = 0x1,
            AI = 0x2,
            Vehicle = 0x4,
            TrapStation = 0x8,
            Special = 0x10,
        }
        
        [Flags]
        public enum LinkType
        {
            Pedestrian = 1,
            AI = 2,
            TramSalinaForward = 4,
            TramSalinaBackward = 0x8400,
            Other = 0x1000
        }
       
        public class Header
        {
            // should be 0x1ABCEDF
            public uint magic;
            public uint numPoints;
        }

        public class Point
        {
            public Vector3 pos;
            public PointType type;
            public ushort ID;
            public ushort areaSize;
            public byte[] unk; //[10];
            public byte enterLinks;
            public byte exitLinks; // equals mEnterLinks
        }

        public class Link
        {
            public ushort targetPoint;
            public ushort linkType;
            public float unk;
        }

        public List<Point> points = new List<Point>();
        public List<Link> links = new List<Link>();

        
        private Point ReadPoint(BinaryReader reader)
        {
            Point newPoint = new Point();
            newPoint.pos = ReadVector3(reader);
            newPoint.type = (PointType)reader.ReadUInt16();
            newPoint.ID = reader.ReadUInt16();
            newPoint.areaSize = reader.ReadUInt16();
            newPoint.unk = new byte[10];

            for (var i = 0; i < 10; i++)
                newPoint.unk[i] = reader.ReadByte();

            newPoint.enterLinks = reader.ReadByte();
            newPoint.exitLinks = reader.ReadByte();
           

            return newPoint;
        }

        private Link ReadLink(BinaryReader reader)
        {
            Link newLink = new Link();
            newLink.targetPoint = reader.ReadUInt16();
            newLink.linkType = reader.ReadUInt16();
            newLink.unk = reader.ReadSingle();

            return newLink;
        }

        public bool Load(BinaryReader reader)
        {
            Header newHeader = new Header();
            newHeader.magic = reader.ReadUInt32();
            newHeader.numPoints = reader.ReadUInt32();

            if (newHeader.magic != 0x1ABCEDF)
                return false;

            uint numLinks = 0;

            for (var i = 0; i < newHeader.numPoints; i++)
            {
                var point = ReadPoint(reader);
                numLinks += point.enterLinks;
                points.Add(point);
            }

            // Each point references 0 or more links.
            // For example, if point 0 has mEnterLinks = 2, it means that the first 2 links belong to it.
            // Consequent links in a row belong to point 1, 2 and so on.

            for (var i = 0; i < numLinks; i++)
                links.Add(ReadLink(reader)); 
                
            return true;
        }
    }
}
