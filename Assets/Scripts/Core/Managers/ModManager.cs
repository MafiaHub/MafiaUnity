using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            if (mods.ContainsKey(modName))
                return mods[modName];

            var newMod = ReadModInfo(modName);

            if (newMod == null)
                return null;
            
            newMod.Init();
            mods.Add(modName, newMod);

            return newMod;
        }

        public void InitializeMods()
        {
            foreach (var mod in mods)
            {
                mod.Value.Start();
            }
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

            var packagePath = Path.Combine(MODS_PATH, modName, "mod.json");

            if (!File.Exists(packagePath))
                return null;

            Mod newMod = new Mod(modName);

            var jsonContents = File.ReadAllText(packagePath);

            try
            {
                JsonUtility.FromJsonOverwrite(jsonContents, newMod);
                return newMod;
            }
            catch
            {
                Debug.LogError("Mod " + modName + " couldn't be loaded, invalid mod.json!");
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

        public string[] GetAllModNames()
        {
            if (!Directory.Exists(MODS_PATH))
                return new string[] { };

            var mods = Directory.GetDirectories(MODS_PATH);

            for (int i = 0; i < mods.Length; i++)
            {
                mods[i] = mods[i].Substring(MODS_PATH.Length);
            }

            return mods;
        }

        public KeyValuePair<string, string>[] GetLoadOrder()
        {
            var loadableMods = new List<KeyValuePair<string, string>>();

            if (!Directory.Exists("Mods"))
                Directory.CreateDirectory("Mods");

            if (!File.Exists(Path.Combine(MODS_PATH, "loadorder.txt")))
                return loadableMods.ToArray();

            var contents = File.ReadAllLines(Path.Combine(MODS_PATH, "loadorder.txt"));

            loadableMods.Add(new KeyValuePair<string, string>("MafiaBase", "1"));

            foreach (var line in contents)
            {
                var vals = line.Split(' ');

                if (vals[0] == "MafiaBase")
                    continue;

                loadableMods.Add(new KeyValuePair<string, string>(vals[0], vals[1]));
            }
            
            return loadableMods.ToArray();
        }

        public void StoreLoadOrder(KeyValuePair<string, string>[] mods)
        {
            var data = new StringBuilder();

            foreach (var mod in mods)
            {
                data.AppendFormat("{0} {1}\r\n", mod.Key, mod.Value);
            }

            try
            {
                File.WriteAllText(Path.Combine(MODS_PATH, "loadorder.txt"), data.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}