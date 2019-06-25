using UnityEngine;
using System;

namespace MafiaUnity.Build
{
    using System;
    using System.IO;
    using UnityEngine;

    public class Info
    {
        private static Info _instance;
        public static Info Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Info();
                }

                return _instance;
            }
        }

        public DateTime BuildTime { get; private set; }

        protected Info()
        {
            byte[] ByteInfo = this.ReadStreamingAsset("BuildInfo");

            // file does not exists. set defaults!
            if (ByteInfo.Length == 0)
            {
                BuildTime = DateTime.UtcNow;

                return;
            }
            // else, read the infos from file

            using (BinaryReader Reader = new BinaryReader(new MemoryStream(ByteInfo, false)))
            {
                BuildTime = DateTime.FromBinary(Reader.ReadInt64());
            }
        }

        public byte[] ReadStreamingAsset(string path)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, path);

            // add file prefix
            if (Application.platform != RuntimePlatform.Android)
            {
                filePath = "file://" + filePath;
            }

            WWW fileContent = new WWW(filePath);

            // wait for file was loaded!
            while (!fileContent.isDone && string.IsNullOrEmpty(fileContent.error)) { }

            return fileContent.bytes;
        }
    }

#if UNITY_EDITOR
    public class AndroidBuildPrepartion : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            using (BinaryWriter Writer = new BinaryWriter(File.Open("Assets/StreamingAssets/BuildInfo", FileMode.Create)))
            {
                Writer.Write(DateTime.UtcNow.ToBinary());
            }
        }
    }
#endif
}