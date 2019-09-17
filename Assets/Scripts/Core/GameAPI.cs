#if UNITY_EDITOR
#undef MAFIAUNITY_RELEASE
#endif

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
                {
                    instanceObject = new GameAPI();
                    instanceObject.InitInternals();
                }

                return instanceObject;
            }
        }

        public static void ResetGameAPI()
        {
            instanceObject = new GameAPI();
            instanceObject.InitInternals();
        }

        private void InitInternals()
        {
            cvarManager.Init();
            modManager.modsPath = Path.Combine(Application.streamingAssetsPath, "Mods");
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

        /// <summary>
        /// Specifies whether to skip loading the main base game (main menu, Mafia game modes, etc).
        /// This is useful for mods that override the startup sequence with their own scripts.
        /// </summary>
        public bool skipLoadingMainGame { get; set; }

        /// <summary>
        /// Blocks out the rest of the mods on the load list from loading.
        /// Useful when a major mod requires the game to be kept vanilla (ex. multiplayer mods, total conversions, ...)
        /// </summary>
        public bool blockMods { get; set; }

        /// <summary>
        /// Avoids loading loose files from the disk.
        /// This ensures data is loaded only from DTA archives.
        /// Data from mods can also be loaded if `blockMods` is disabled.
        /// </summary>
        public bool avoidLooseFiles { get; set; }

        #region Public Fields
        public FileSystem fileSystem = new FileSystem();
        public CvarManager cvarManager = new CvarManager();
        public ConsoleManager consoleManager = new ConsoleManager();
        public MissionManager missionManager = new MissionManager();
        public ModManager modManager = new ModManager();

        /// <summary>
        /// An instance of a `Game Instance` game object. This is set from SetupGUI for use in the game.
        /// </summary>
        public GameObject gameInstance = null;

        public ModelGenerator modelGenerator = new ModelGenerator();
        public CityGenerator cityGenerator = new CityGenerator();
        public SceneGenerator sceneGenerator = new SceneGenerator();
        #endregion

        private bool isInitialized = false;
        private bool modHadErrors = false;

        /// <summary>
        /// Returns whether the Game Manager was already initialized or not
        /// </summary>
        /// <returns></returns>
        public bool GetInitialized()
        {
            return isInitialized;
        }

        /// <summary>
        /// Returns whether any of loaded mods had script errors detected.
        /// </summary>
        /// <returns></returns>
        public bool GetModErrorStatus()
        {
            return modHadErrors;
        }

        /// <summary>
        /// Setter for modHadErrors
        /// </summary>
        /// <param name="state"></param>
        public void SetModErrorStatus(bool state)
        {
            modHadErrors = true;
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
                isInitialized = true;

                return true;
            }

            return false;
        }
    }
}
