using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace MafiaUnity
{
    public struct LoadingScreen
    {
        public string missionName;
        public string fileName;
        public uint textId;
    }

    public class LoadDEFLoader : BaseLoader
    {
        public List<LoadingScreen> load(BinaryReader reader)
        {
            List<LoadingScreen> loadingScreens = new List<LoadingScreen>();

            while (true)
            {
                var newLoadingScreen = new LoadingScreen();

                try
                {
                    newLoadingScreen.fileName = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(32));
                    newLoadingScreen.missionName = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(32));
                    newLoadingScreen.textId = reader.ReadUInt32();
                }
                catch
                {
                    return loadingScreens;
                }

                loadingScreens.Add(newLoadingScreen);
            }

            return loadingScreens;
        }
    }
}
