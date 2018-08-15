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
            public fixed char magic[4];
            public uint unknown;
            public uint numControls;
        }

        public unsafe struct Control
        {
            public uint unknown;
            public fixed char type[4];
            Vector2 pos;
            float scaleX;
            float scaleY;
            uint textId;
            ushort textColor;
            ushort bgColor;
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

            var menuString = new string(newHeader.magic);

            if (menuString != "Menu") 
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
