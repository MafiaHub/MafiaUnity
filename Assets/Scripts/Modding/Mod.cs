using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    [Serializable]
    public class Mod
    {
        string modName;
        public string name;
        public string author;
        public string version;
        public string gameVersion;

        public Mod(string name)
        {
            modName = Path.Combine("mods", name, "data");
        }

        [SerializeField] List<string> dependencies;

        public void Init()
        {
            GameManager.instance.fileSystem.AddOptionalPath(modName);
        }

        public void Destroy()
        {
            GameManager.instance.fileSystem.RemoveOptionalPath(modName);
        }
    }

    public class ModEntry
    {
        public string modName;
        public Mod modMeta;
        public int isActive;
    }
}