using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    public class ModManager
    {
        private const string modsPath = "Mods/";

        Dictionary<string, Mod> mods = new Dictionary<string, Mod>();

        public Mod LoadModification(string modName)
        {
            if (mods.ContainsKey(modName))
                return mods[modName];

            if (!Directory.Exists(Path.Combine(modsPath, modName)))
                return null;

            if (!File.Exists(Path.Combine(modsPath, modName, "package.json")))
                return null;

            Mod newMod = new Mod();

            return null;
        }
    }
}