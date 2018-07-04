using B83.Image.BMP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenMafia
{

    [Serializable]
    public class GameManager
    {

        #region Singleton
        static GameManager instanceObject;
        public static GameManager instance
        {
            get
            {
                if (instanceObject == null)
                    instanceObject = new GameManager();
                
                return instanceObject;
            }
        }
        #endregion

        /// <summary>
        /// This constant is bumped each time we ship new build.
        /// </summary>
        public const int GAME_VERSION = 1;

        #region Public Fields
        public FileSystem fileSystem = new FileSystem();
        public CvarManager cvarManager = new CvarManager();
        public ConsoleManager consoleManager = new ConsoleManager();
        public MissionManager missionManager = new MissionManager();

        public ModelGenerator modelGenerator = new ModelGenerator();
        public CityGenerator cityGenerator = new CityGenerator();
        public SceneGenerator sceneGenerator = new SceneGenerator();
        #endregion

        private bool isInitialized = false;

        /// <summary>
        /// Wrapper method which sets the game path as well as initializes the CvarManager object.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool SetGamePath(string path)
        {
            if (isInitialized)
                return true;

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