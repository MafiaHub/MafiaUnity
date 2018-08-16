using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    namespace MafiaFormats
    {
        public class MNULoader : BaseLoader
        {
            public class Header
            {
                public char[] magic; // 4 bytes
                public uint unknown;
                public uint numControls;
            }

            public class Control
            {
                public uint unknown;
                public string type;
                public Vector2 pos;
                public float scaleX;
                public float scaleY;
                public uint textId;
                public ushort textColor;
                public ushort bgColor;
            }

            public class OldControl
            {
                public uint unknown;
                public string type;
                public Vector2 pos;
                public float scaleX;
                public float scaleY;
                public uint textId;
                public uint textColor;
                public uint bgColor;
            }

            public List<Control> controls = new List<Control>();

            private bool isOldVersion = false;

            public bool GetVersion() { return isOldVersion; }

            Header ReadHeader(BinaryReader reader)
            {
                Header header = new Header();

                header.magic = reader.ReadChars(4);
                header.unknown = reader.ReadUInt32();
                header.numControls = reader.ReadUInt32();

                return header;
            }

            Control ReadControl(BinaryReader reader)
            {
                Control control = new Control();

                control.unknown = reader.ReadUInt32();
                control.type = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));
                control.pos = ReadVector2(reader);
                control.scaleX = reader.ReadSingle();
                control.scaleY = reader.ReadSingle();
                control.textId = reader.ReadUInt32();
                control.textColor = reader.ReadUInt16();
                control.bgColor = reader.ReadUInt16();

                return control;
            }

            OldControl ReadOldControl(BinaryReader reader)
            {
                OldControl control = new OldControl();

                control.unknown = reader.ReadUInt32();
                control.type = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));
                control.pos = ReadVector2(reader);
                control.scaleX = reader.ReadSingle();
                control.scaleY = reader.ReadSingle();
                control.textId = reader.ReadUInt32();
                control.textColor = reader.ReadUInt32();
                control.bgColor = reader.ReadUInt32();

                return control;
            }

            public bool Load(BinaryReader reader, bool isOldVersion=false)
            {
                this.isOldVersion = isOldVersion;

                if (!isOldVersion)
                {
                    Header newHeader = ReadHeader(reader);

                    var menuString = new string(newHeader.magic);

                    if (menuString != "Menu") 
                    {
                        return false;
                    }

                    for (var i = 0; i < newHeader.numControls; i++)
                    {
                        Control newControl = ReadControl(reader);
                        controls.Add(newControl);
                    }
                }
                else
                {
                    long controlCount = reader.BaseStream.Length / 36; // Size of file divided by size of OldControl

                    for (var i = 0; i < controlCount; i++)
                    {
                        OldControl oldControl = ReadOldControl(reader);
                        controls.Add(ConvertOldControl(oldControl));
                    }
                }

                return true;
            }

            private Control ConvertOldControl(OldControl old)
            {
                Control ctl = new Control();

                ctl.unknown = old.unknown;
                ctl.type = old.type;
                ctl.pos = old.pos;
                ctl.scaleX = old.scaleX;
                ctl.scaleY = old.scaleY;
                ctl.textId = old.textId;
                ctl.textColor = (ushort)old.textColor;
                ctl.bgColor = (ushort)old.bgColor;

                return ctl;
            }
        }
    }
}
