using UnityEngine;
using System.Collections;

namespace MafiaFormats
{
    public partial struct Vector3
    {
        public static implicit operator UnityEngine.Vector3(Vector3 rhs) => new UnityEngine.Vector3(rhs.x, rhs.y, rhs.z);
    }

    public partial struct Vector2
    {
        public static implicit operator UnityEngine.Vector2(Vector2 rhs) => new UnityEngine.Vector2(rhs.x, rhs.y);
    }

    public partial struct Quaternion
    {
        public static implicit operator UnityEngine.Quaternion(Quaternion rhs) => new UnityEngine.Quaternion(rhs.x, rhs.y, rhs.z, rhs.w);
    }

    public partial class Matrix4x4
    {
        public static implicit operator UnityEngine.Matrix4x4(Matrix4x4 rhs)
        {
            var newMatrix = new UnityEngine.Matrix4x4();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    newMatrix[i, j] = rhs[i, j];
                }
            }

            return newMatrix;
        }
    }
}