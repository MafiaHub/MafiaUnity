using B83.Image.BMP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MafiaUnity
{

    [Serializable]
    public class GameAPI
    {

        #region Singleton
        static GameAPI instanceObject;
        public static GameAPI instance
        {
            get
            {
                if (instanceObject == null)
                    instanceObject = new GameAPI();
                
                return instanceObject;
            }
        }

        public static void ResetGameAPI()
        {
            instanceObject = new GameAPI();
        }
        #endregion

        /// <summary>
        /// This constant is a version string we bump up each time we ship a new build.
        /// Minor part gets incremented each shipped update, while Major part has to be incremented only
        /// when framework's API radically changes.
        /// </summary>

#if MAFIAUNITY_RELEASE
        public const string GAME_VERSION = "v1.0.0";
#else
        public const string GAME_VERSION = "INDEV";
#endif

        public bool isPaused = false;

        #region Public Fields
        public FileSystem fileSystem = new FileSystem();
        public CvarManager cvarManager = new CvarManager();
        public ConsoleManager consoleManager = new ConsoleManager();
        public MissionManager missionManager = new MissionManager();
        public ModManager modManager = new ModManager();

        public ModelGenerator modelGenerator = new ModelGenerator();
        public CityGenerator cityGenerator = new CityGenerator();
        public SceneGenerator sceneGenerator = new SceneGenerator();
        #endregion

        private bool isInitialized = false;

        /// <summary>
        /// Returns whether the Game Manager was already initialized or not
        /// </summary>
        /// <returns></returns>
        public bool GetInitialized()
        {
            return isInitialized;
        }

        /// <summary>
        /// Wrapper method which sets the game path as well as initializes the CvarManager object.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool SetGamePath(string path)
        {
            if (isInitialized)
                return fileSystem.ValidateGamePath(path);

            if (fileSystem.SetGamePath(path))
            {
                cvarManager.configPath = fileSystem.gamePath;
                cvarManager.Init();

                isInitialized = true;

                return true;
            }

            return false;
        }
    }
}