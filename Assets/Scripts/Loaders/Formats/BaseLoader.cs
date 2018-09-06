using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace MafiaUnity
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

        public static void WriteQuat(BinaryWriter writer, Quaternion quat)
        {
            writer.Write(quat.x);
            writer.Write(quat.y);
            writer.Write(quat.z);
            writer.Write(quat.w);
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

        public static void WriteVector3(BinaryWriter writer, Vector3 vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
            writer.Write(vec.z);
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

        public static string ReadStringUInt32(BinaryReader reader)
        {
            var length = reader.ReadUInt32();
            return System.Text.Encoding.ASCII.GetString(reader.ReadBytes((int)length));
        }

        public static string ReadString(BinaryReader reader, int length)
        {
            return System.Text.Encoding.ASCII.GetString(reader.ReadBytes(length));
        }

        public static void WriteStringUInt32(BinaryWriter writer, string text)
        {
            writer.Write((uint)text.Length);
            writer.Write(System.Text.Encoding.ASCII.GetBytes(text));
        }
        
        public static string ReadTerminatedString(BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();

            int status = 0;

            while ((status = reader.ReadByte()) != '\0')
            {
                char c = (char)status;
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
