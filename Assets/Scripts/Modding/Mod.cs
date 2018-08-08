using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace MafiaUnity
{
    [Serializable]
    public class Mod
    {
        string modPath, modName;
        public string name;
        public string author;
        public string version;
        public string gameVersion;
        public Assembly assembly;
        [SerializeField] public List<string> dependencies;

        public Mod(string name)
        {
            modName = name;
            modPath = Path.Combine("Mods", name);
        }
        
        public void Init()
        {
            GameManager.instance.fileSystem.AddOptionalPath(modPath);

            var scriptsPath = Path.Combine(modPath, "Scripts");

            if (Directory.Exists(scriptsPath))
            {
                var fileNames = Directory.GetFiles(scriptsPath);
                var sources = new List<string>();

                foreach (var fileName in fileNames)
                {
                    if (File.Exists(fileName) && Path.GetExtension(fileName) == ".cs")
                        sources.Add(File.ReadAllText(fileName));
                }

                if (sources.Count < 1)
                    return;

                assembly = Compiler.CompileSource(sources.ToArray(), true);

                if (assembly == null)
                {
                    Debug.LogError("Assembly for " + modName + " couldn't be compiled!");
                    return;
                }

                var allTypes = Compiler.GetLoadableTypes(assembly);

                foreach (var type in allTypes)
                {
                    if (type.ToString() == "ScriptMain")
                    {
                        IModScript entry = (IModScript)assembly.CreateInstance(type.ToString(), true);

                        if (entry == null)
                            break;

                        entry.Start(this);

                        break;
                    }
                }
            }
        }

        public void Destroy()
        {
            GameManager.instance.fileSystem.RemoveOptionalPath(modPath);
        }
    }

    public class ModEntry
    {
        public string modName;
        public Mod modMeta;
        public int isActive;
    }
}