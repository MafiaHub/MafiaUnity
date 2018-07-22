using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    public class DTAFileSystem
    {
        public static void MountDTAFiles()
        {
            //LoadFilesFromDTA("A0.dta", 0xD8D0A975, 0x467ACDE0);
            LoadFilesFromDTA("A1.dta", 0x3D98766C, 0xDE7009CD);
            LoadFilesFromDTA("A2.dta", 0x82A1C97B, 0x2D5085D4);
            LoadFilesFromDTA("A3.dta", 0x43876FEA, 0x900CDBA8);
            LoadFilesFromDTA("A4.dta", 0x43876FEA, 0x900CDBA8);
            LoadFilesFromDTA("A5.dta", 0xDEAC5342, 0x760CE652);
            LoadFilesFromDTA("A6.dta", 0x64CD8D0A, 0x4BC97B2D);
            LoadFilesFromDTA("A7.dta", 0xD6FEA900, 0xCDB76CE6);
            LoadFilesFromDTA("A8.dta", 0xD8DD8FAC, 0x5324ACE5);
            LoadFilesFromDTA("A9.dta", 0x6FEE6324, 0xACDA4783);
            LoadFilesFromDTA("AA.dta", 0x5342760C, 0xEDEAC652);
            LoadFilesFromDTA("AB.dta", 0xD8D0A975, 0x467ACDE0);
            LoadFilesFromDTA("AC.dta", 0x43876FEA, 0x900CDBA8);
        }

        public static Dictionary<string, DTALoader> dtaFiles = new Dictionary<string, DTALoader>();

        public static void LoadFilesFromDTA(string dtaFileName, uint key1, uint key2)
        {
            var dtaReader = new DTALoader(key1, key2);

            FileStream fs;

            try
            {
                fs = new FileStream("D:\\Mafia 1.2\\" + dtaFileName, FileMode.Open);
            }
            catch
            {
                return;
            }

            var reader = new BinaryReader(fs);
            dtaReader.Load(ref reader);

            foreach (var fileName in dtaReader.fileNames)
            {
                var lowered = fileName.ToLower();

                if (dtaFiles.ContainsKey(lowered))
                    dtaFiles.Remove(lowered);

                dtaFiles.Add(lowered, dtaReader);
            }

            Debug.Log(dtaFileName);
        }

        public static Stream GetFileContent(string file)
        {
            var fileName = file.ToLower().Replace(@"/", @"\");

            if (dtaFiles.ContainsKey(fileName))
            {
                return new MemoryStream(dtaFiles[fileName].GetFile(fileName));
            }
            else Debug.Log("Unable to get: " + fileName);

            return null;
        }

        public static bool FileExists(string file)
        {
            var fileName = file.ToLower().Replace(@"/", @"\");
            return dtaFiles.ContainsKey(fileName);
        }
    }
}
