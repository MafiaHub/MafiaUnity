using System.Collections;

namespace MafiaFormats
{
    public partial struct Vector3
    {
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public float x,y,z;
    }

    public partial struct Vector2
    {
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public float x, y;
    }

    public partial class Matrix4x4
    {
        public Matrix4x4()
        {
            values = new float[4, 4];
        }

        public float this[int i, int j]
        {
            get { return values[i, j]; }
            set { values[i, j] = value; }
        }

        public float[,] values;
    }

    public partial struct Quaternion
    {
        public Quaternion(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public float w, x, y, z;
    }
}
