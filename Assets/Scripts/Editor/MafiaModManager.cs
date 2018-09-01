using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MafiaUnity
{
#if UNITY_EDITOR
    /// <summary>
    /// Example implementation of Mod Manager GUI as well as a test ground for loading mods inside of editor.
    /// </summary>
    [Serializable]
    public class MafiaModManager : EditorWindow
    {
        ModManager modManager;

        List<ModEntry> modEntries;
        int selectedModIndex = 0;

        bool isInitialized = false;

        public void Init()
        {
            titleContent = new GUIContent("Mod Manager");
            Show();
        }

        private void OnGUI()
        {
            if (!GameAPI.instance.GetInitialized())
            {
                isInitialized = false;
                GUILayout.Label("Game manager is not initialized yet.");
                return;
            }

            if (!isInitialized)
            {
                modManager = GameAPI.instance.modManager;
                isInitialized = true;

                var modNames = new List<string>(modManager.GetAllModNames());
                var mods = new List<ModEntry>();
                modEntries = new List<ModEntry>();

                foreach (var modName in modNames)
                {
                    var modEntry = new ModEntry();
                    modEntry.modMeta = modManager.ReadModInfo(modName);
                    modEntry.modName = modName;
                    modEntry.status = 0;
                    mods.Add(modEntry);
                }

                var newMods = new List<ModEntry>(mods);

                var loadOrder = modManager.GetLoadOrder();

                foreach (var load in loadOrder)
                {
                    foreach (var mod in mods)
                    {
                        if (mod.modName == load.Key)
                        {
                            if (load.Value == "1")
                                mod.status = ModEntryStatus.Active;

                            modEntries.Add(mod);
                            newMods.Remove(mod);
                        }
                    }
                }

                foreach (var newMod in newMods)
                    modEntries.Add(newMod);

                ApplyChanges();
            }

            if (modEntries == null)
                return;

            for (var i = 0; i < modEntries.Count; i++)
            {
                var modEntry = modEntries[i];

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        selectedModIndex = i;
                    }

                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        if (i + 1 < modEntries.Count)
                        {
                            var oldMod = modEntries[i + 1];
                            modEntries[i] = oldMod;
                            modEntries[i + 1] = modEntry;
                        }
                    }

                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        if (i > 0)
                        {
                            var oldMod = modEntries[i - 1];
                            modEntries[i] = oldMod;
                            modEntries[i - 1] = modEntry;
                        }
                    }

                    modEntry.status = GUILayout.Toggle(modEntry.status == ModEntryStatus.Active, "Active") ? ModEntryStatus.Active : ModEntryStatus.Inactive;

                    EditorGUILayout.LabelField(modEntry.modName);
                }
                GUILayout.EndHorizontal();
            }

            if (modEntries.Count > 0 && selectedModIndex >= 0 && selectedModIndex < modEntries.Count)
            {
                var mod = modEntries[selectedModIndex].modMeta;

                if (mod == null)
                {
                    GUILayout.Label("Error loading mod metadata!");
                }
                else
                {
                    GUILayout.Label("Name: " + mod.name);
                    GUILayout.Label("Author: " + mod.author);
                    GUILayout.Label("Version: " + mod.version);
                    GUILayout.Label("Game Version: " + mod.gameVersion);
                    GUILayout.Label("Depends on:");

                    foreach (var dep in mod.dependencies)
                    {
                        GUILayout.Label("  - " + dep);
                    }
                }
            }


            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply Changes"))
                {
                    ApplyChanges();
                }

                if (GUILayout.Button("Initialize Mods"))
                {
                    ApplyChanges();

                    foreach (var mod in modEntries)
                    {
                        if (mod.status != 0)
                        {
                            modManager.LoadMod(mod.modName);
                        }
                    }

                    modManager.InitializeMods();
                }
            }
            GUILayout.EndHorizontal();

        }

        void ApplyChanges()
        {
            var newLoadOrder = new List<KeyValuePair<string, string>>();

            foreach (var mod in modEntries)
            {
                newLoadOrder.Add(new KeyValuePair<string, string>(mod.modName, mod.status == ModEntryStatus.Active ? "1" : "0"));
            }

            modManager.StoreLoadOrder(newLoadOrder.ToArray());
        }
    }
#endif
}