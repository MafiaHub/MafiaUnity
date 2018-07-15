using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    class MNULoader : BaseLoader
    {
        public unsafe struct Header
        {
            public fixed byte magic[4];
            public uint unknown;
            public uint numControls;
            public uint padding;
        }

        public unsafe struct Control
        {
            public uint id;
            public fixed byte type[4];
            public Vector2 pos;
            public float scaleX;
            public float scaleY;
            public uint textId;
            public ushort textColor;
            public ushort bgColor;
        }

        public List<Control> controls = new List<Control>();

        private unsafe void ReadStruct(byte* structBytes, byte[] stream)
        {
            for(var i = 0; i < stream.Length; i++)
            {
                structBytes[i] = stream[i];
            }
        }

        public unsafe void Load(BinaryReader reader)
        {
            Header newHeader = new Header();
            ReadStruct((byte*)&newHeader, reader.ReadBytes(sizeof(Header)));

            if (newHeader.magic[0] != 'M' || newHeader.magic[3] != 'u') 
            {
                Debug.Log("Wrong mnu file !");
                return;
            }

            
            for (var i = 0; i < newHeader.numControls; i++)
            {
                Control newControl = new Control();
                ReadStruct((byte*)&newControl, reader.ReadBytes(sizeof(Control)));

                controls.Add(newControl);
            }
        }
    }
}
