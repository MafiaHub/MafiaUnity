using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    /// <summary>
    /// Mod manager handles mod parsing and activation. This class only stores active mods here and shouldn't be used
    /// for loading inactive mods.
    /// </summary>
    public class ModManager
    {
        private const string MODS_PATH = "Mods/";

        Dictionary<string, Mod> mods = new Dictionary<string, Mod>();

        /// <summary>
        /// Load mod into the game, which makes it active.
        /// </summary>
        /// <param name="modName"></param>
        /// <returns></returns>
        public Mod LoadMod(string modName)
        {
            var newMod = ReadModInfo(modName);

            if (newMod == null)
                return null;
            
            newMod.Init();
            mods.Add(modName, newMod);

            return newMod;
        }

        /// <summary>
        /// Removes an active mod from the list.
        /// </summary>
        /// <param name="modName"></param>
        public void RemoveMod(string modName)
        {
            if (!mods.ContainsKey(modName))
                return;

            var mod = mods[modName];

            mod.Destroy();

            mods.Remove(modName);
        }

        /// <summary>
        /// Reads mod info, but doens't make it active nor loaded. Useful when you want to read about mod itself.
        /// </summary>
        /// <param name="modName"></param>
        /// <returns></returns>
        public Mod ReadModInfo(string modName)
        {
            if (mods.ContainsKey(modName))
                return mods[modName];

            if (!Directory.Exists(Path.Combine(MODS_PATH, modName)))
                return null;

            var packagePath = Path.Combine(MODS_PATH, modName, "package.json");

            if (!File.Exists(packagePath))
                return null;

            Mod newMod = new Mod();

            var jsonContents = File.ReadAllText(packagePath);

            try
            {
                JsonUtility.FromJsonOverwrite(jsonContents, newMod);
                return newMod;
            }
            catch
            {
                Debug.LogError("Mod " + modName + " couldn't be loaded, invalid package.json!");
                return null;
            }
        }

        /// <summary>
        /// Retrieves active mod from the dictionary.
        /// </summary>
        /// <param name="modName"></param>
        /// <returns></returns>
        public Mod GetActiveMod(string modName)
        {
            if (mods.ContainsKey(modName))
                return mods[modName];

            return null;
        }
    }
}