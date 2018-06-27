using B83.Image.BMP;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenMafia
{
    public class GameManager
    {

        #region Singleton
        static GameManager instanceObject;
        public static GameManager instance { get { if (instanceObject == null) instanceObject = new GameManager(); return instanceObject; } }
        #endregion

        public const int GAME_VERSION = 1;
        
        public string gamePath { get; private set; }

        public CvarManager cvarManager = new CvarManager();

        public ModelGenerator modelGenerator = new ModelGenerator();
        public CityGenerator cityGenerator = new CityGenerator();
        public SceneGenerator sceneGenerator = new SceneGenerator();

        public bool SetGamePath(string path)
        {
            path = FixGamePath(path);

            if (ValidateGamePath(path))
            {
                gamePath = path;
                cvarManager.configPath = gamePath;
                cvarManager.Init();

                return true;
            }

            return false;
        }

        string FixGamePath(string path)
        {
            if (!path.EndsWith("/"))
                return path + "/";

            return path;
        }

        bool ValidateGamePath(string path)
        {
            // TODO: Validate if game files are present there.
            return true;
        }
    }
}