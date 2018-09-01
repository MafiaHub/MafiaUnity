using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

namespace MafiaUnity
{
#if UNITY_EDITOR
    public class MafiaEditor : EditorWindow
    {
        public string gamePath = "F:/Games/Mafia/";
        public string modelPath = "models/taxi00.4ds";
        public string cityPath = "missions/freeride/cache.bin";
        public string missionName = "freeride";

        bool isInitialized = false;
        string modPath = "";

        public void Init()
        {
            titleContent = new GUIContent("Mafia Editor");
            LoadConfig();
            Show();
        }

        void ReadConfig()
        {
            gamePath = GameAPI.instance.cvarManager.Get("gamePath", gamePath);
            missionName = GameAPI.instance.cvarManager.Get("editorMissionName", missionName);
        }

        private void OnGUI()
        {
            if (!isInitialized && GameAPI.instance.cvarManager.values != null)
            {
                ReadConfig();
                GameAPI.instance.SetGamePath(gamePath);
                isInitialized = true;
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Game Path");
                gamePath = EditorGUILayout.TextField(gamePath);

                if (GUILayout.Button("Browse"))
                {
                    gamePath = EditorUtility.OpenFolderPanel("Select game directory...", "mafia", gamePath);
                }

                if (GUILayout.Button("Set Path"))
                {
                    GameAPI.instance.SetGamePath(gamePath);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                modPath = EditorGUILayout.TextField(modPath);
                
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    GameAPI.instance.fileSystem.AddOptionalPath(modPath);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            foreach (var p in GameAPI.instance.fileSystem.GetAllPaths())
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        GameAPI.instance.fileSystem.RemoveOptionalPath(p.Replace("/Data", ""));
                    }

                    GUILayout.Label(p);
                }
                EditorGUILayout.EndHorizontal();
            }   

            EditorGUILayout.BeginHorizontal();
            {
                modelPath = EditorGUILayout.TextField(modelPath);

                if (GUILayout.Button("Spawn Object"))
                {
                    GameAPI.instance.SetGamePath(gamePath);

                    GameAPI.instance.modelGenerator.LoadObject(modelPath, null);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                cityPath = EditorGUILayout.TextField(cityPath);

                if (GUILayout.Button("Spawn City"))
                {
                    GameAPI.instance.SetGamePath(gamePath);

                    GameAPI.instance.cityGenerator.LoadObject(cityPath, null);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                missionName = EditorGUILayout.TextField(missionName);

                if (GUILayout.Button("Spawn Mission"))
                {
                    GameAPI.instance.SetGamePath(gamePath);
                    GameAPI.instance.missionManager.LoadMission(missionName);
                }

                if (GUILayout.Button("Append Mission"))
                {
                    GameAPI.instance.SetGamePath(gamePath);
                    GameAPI.instance.missionManager.LoadMission(missionName, true);
                }

                if (GUILayout.Button("Destroy Mission"))
                {
                    GameAPI.instance.missionManager.DestroyMission();

                    var leftover = GameObject.Find(missionName);

                    if (leftover != null)
                    {
                        GameObject.DestroyImmediate(leftover, true);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            if (GUILayout.Button("Save Game Config"))
            {
                var cvars = GameAPI.instance.cvarManager;
                cvars.ForceSet("gamePath", gamePath, CvarManager.CvarMode.Archived);
                cvars.ForceSet("editorMissionName", missionName, CvarManager.CvarMode.Archived);
                cvars.SaveMainConfig();
            }

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save Editor"))
                {
                    SaveConfig();
                }

                if (GUILayout.Button("Load Editor"))
                {
                    LoadConfig();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void SaveConfig()
        {
            var cfg = JsonUtility.ToJson(this);
            File.WriteAllText(EDITOR_CFG, cfg);
        }

        void LoadConfig()
        {
            if (File.Exists(EDITOR_CFG))
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(EDITOR_CFG), this);
            }
        }

        const string EDITOR_CFG = "Assets/External Assets/editor.json";
    }
#endif
}