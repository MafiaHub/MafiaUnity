using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MafiaFormats
{
    public class LoadingScreen
    {
        public string missionName;
        public string fileName;
        public uint textId;
    }

    public partial class LoadDEFLoader : BaseLoader
    {
        public List<LoadingScreen> load(BinaryReader reader)
        {
            List<LoadingScreen> loadingScreens = new List<LoadingScreen>();

            var count = reader.BaseStream.Length / (64 + sizeof(uint));

            for (var i = 0; i < count; i++)
            {
                var newLoadingScreen = new LoadingScreen();

                newLoadingScreen.fileName = ReadString(reader, 32);
                newLoadingScreen.missionName = ReadString(reader, 32);
                newLoadingScreen.textId = reader.ReadUInt32();

                loadingScreens.Add(newLoadingScreen);
            }

            return loadingScreens;
        }
    }
}
