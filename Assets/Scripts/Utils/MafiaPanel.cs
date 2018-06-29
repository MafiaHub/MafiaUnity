using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

namespace OpenMafia
{
#if UNITY_EDITOR
    public class MafiaEditor : EditorWindow
    {
        public string gamePath = "F:/Games/Mafia/";
        public string modelPath = "models/taxi00.4ds";
        public string cityPath = "missions/freeride/cache.bin";
        public string missionName = "freeride";

        bool isInitialized = false;
        int modPathSel = 0;
        string modPath = "";

        public static void Init()
        {
            var window = GetWindow<MafiaEditor>();
            window.titleContent = new GUIContent("Mafia Editor");
            window.LoadConfig();
            window.Show();
        }

        void ReadConfig()
        {
            gamePath = GameManager.instance.cvarManager.Get("gamePath", gamePath);
            missionName = GameManager.instance.cvarManager.Get("editorMissionName", missionName);
        }

        private void OnGUI()
        {
            if (!isInitialized && GameManager.instance.cvarManager.values != null)
            {
                ReadConfig();
                GameManager.instance.SetGamePath(gamePath);
                isInitialized = true;
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Game Path");
                gamePath = EditorGUILayout.TextField(gamePath);

                if (GUILayout.Button("Set Path"))
                {
                    GameManager.instance.SetGamePath(gamePath);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                modPath = EditorGUILayout.TextField(modPath);
                
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    GameManager.instance.fileSystem.AddOptionalPath(modPath);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            foreach (var p in GameManager.instance.fileSystem.GetAllPaths())
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        GameManager.instance.fileSystem.RemoveOptionalPath(p);
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
                    GameManager.instance.SetGamePath(gamePath);

                    GameManager.instance.modelGenerator.LoadObject(modelPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                cityPath = EditorGUILayout.TextField(cityPath);

                if (GUILayout.Button("Spawn City"))
                {
                    GameManager.instance.SetGamePath(gamePath);

                    GameManager.instance.cityGenerator.LoadObject(cityPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                missionName = EditorGUILayout.TextField(missionName);

                if (GUILayout.Button("Spawn Mission"))
                {
                    GameManager.instance.SetGamePath(gamePath);
                    GameManager.instance.missionManager.LoadMission(missionName);
                }

                if (GUILayout.Button("Destroy Mission"))
                {
                    GameManager.instance.missionManager.DestroyMission();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Open Object Injector"))
            {
                MafiaObjectInjector.Init();
            }


            GUILayout.Space(15);

            if (GUILayout.Button("Save Game Config"))
            {
                var cvars = GameManager.instance.cvarManager;
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
    

    public class MafiaObjectInjector : EditorWindow
    {
        public List<ObjectInjector> injectors = new List<ObjectInjector>();

        private int selectedInjector;
        private string missionName;
        private string objectName;

        public static void Init()
        {
            var window = GetWindow<MafiaObjectInjector>();
            window.titleContent = new GUIContent("Object Injector");
            window.Show();
        }

        private void OnGUI()
        {
            if (injectors.Count == 0)
            {
                GUILayout.Label("No injectors available");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var injectorOptions = from inj in injectors select inj.requestedMissionName;
                    selectedInjector = EditorGUILayout.Popup("Injectors", selectedInjector, injectorOptions.ToArray());

                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        injectors.RemoveAt(selectedInjector);

                        if (selectedInjector != 0 && selectedInjector == injectors.Count)
                            selectedInjector--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            {
                missionName = GUILayout.TextField(missionName);

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    injectors.Add(new ObjectInjector(missionName));
                }

                if (GUILayout.Button("Add Current Mission"))
                {
                    injectors.Add(new ObjectInjector(GameManager.instance.missionManager.mission.missionName));
                }
            }
            EditorGUILayout.EndHorizontal();

            if (injectors.Count == 0)
                return;

            GUILayout.Space(15);

            var injector = injectors[selectedInjector];

            EditorGUILayout.BeginHorizontal();
            {
                objectName = GUILayout.TextField(objectName);

                if (GUILayout.Button("Add Object"))
                {
                    injector.tasks.Add(new EditorObjectTask { name = objectName });
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            var objectsToRemove = new List<EditorObjectTask>();

            foreach (var task in injector.tasks)
            {
                var editorTask = task as EditorObjectTask;

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        objectsToRemove.Add(editorTask);
                    }

                    GUILayout.Label(task.name);

                    task.findMode = (ObjectInjector.ObjectFindMode)EditorGUILayout.Popup((int)task.findMode, new string[] { "Equals", "Contains", "StartsWith", "EndsWith" });

                    editorTask.popupSelection = EditorGUILayout.Popup(editorTask.popupSelection, new string[] { "None", "Destroy", "Custom..." });

                    if (GUILayout.Button("Apply Modifier"))
                    {
                        if (editorTask.popupSelection == 0)
                            task.task = null;

                        else if (editorTask.popupSelection == 1)
                            task.task = ObjectInjector.TaskDestroy;

                        else
                        {
                            System.Type type = typeof(BaseTaskList);

                            MethodInfo mi = type.GetMethod("TaskExample", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                            if (mi != null)
                            {
                                task.task = (Action<GameObject[]>)Delegate.CreateDelegate(typeof(Action<GameObject[]>), typeof(GameObject[]), mi);
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Inject Now"))
            {
                injector.InjectMissionLoaded(GameManager.instance.missionManager.mission.missionName);
            }

            objectsToRemove.ForEach(x => injector.tasks.Remove(x));
        }
    }

    public class EditorObjectTask : ObjectInjector.ObjectTask
    {
        public int popupSelection;
    }

#endif
}