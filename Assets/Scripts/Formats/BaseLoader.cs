using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenMafia
{
    public class BaseLoader
    {
        public static Quaternion ReadQuat(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            return new Quaternion(x, y, z, w);
        }

        public static Vector3 ReadVector3(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }

        public static Vector2 ReadVector2(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            return new Vector2(x, y);
        }

        public static Matrix4x4 ReadMatrix(BinaryReader reader)
        {
            Matrix4x4 returnMatrix = new Matrix4x4();
            for (var y = 0; y < 4; y++)
                for (var x = 0; x < 4; x++)
                    returnMatrix[x, y] = reader.ReadSingle();

            return returnMatrix;
        }

        public static string ReadString(BinaryReader reader)
        {
            var length = reader.ReadByte();
            return System.Text.Encoding.ASCII.GetString(reader.ReadBytes(length));
        }
    }
}
