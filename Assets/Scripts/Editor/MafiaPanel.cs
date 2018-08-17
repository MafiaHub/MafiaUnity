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
                        GameManager.instance.fileSystem.RemoveOptionalPath(p.Replace(Path.DirectorySeparatorChar+"Data", ""));
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

                    var leftover = GameObject.Find(missionName);

                    if (leftover != null)
                    {
                        GameObject.DestroyImmediate(leftover, true);
                    }
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

        public static void Init()
        {
            var window = GetWindow<MafiaModManager>();
            window.titleContent = new GUIContent("Mod Manager");
            window.Show();
        }

        private void OnGUI()
        {
            if (!GameManager.instance.GetInitialized())
            {
                isInitialized = false;
                GUILayout.Label("Game manager is not initialized yet.");
                return;
            }

            if (!isInitialized)
            {
                modManager = GameManager.instance.modManager;
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

    [Serializable]
    public class MafiaGenerateSolution : EditorWindow
    {
        public static void Init()
        {
            var window = GetWindow<MafiaGenerateSolution>();
            window.titleContent = new GUIContent("Solution Generation");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                unityPath = GUILayout.TextField(unityPath);

                if (GUILayout.Button("Browse"))
                {
                    unityPath = EditorUtility.OpenFolderPanel("Select Unity installation directory", "", "");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                solutionPath = GUILayout.TextField(solutionPath);

                if (GUILayout.Button("Browse"))
                {
                    solutionPath = EditorUtility.OpenFolderPanel("Select output directory", "", "");
                    solutionName = Path.GetFileName(Directory.GetParent(solutionPath).FullName);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                solutionName = GUILayout.TextField(solutionName);

                if (GUILayout.Button("Generate"))
                {
                    Debug.Log("Generating .sln file...");

                    // NOTE: We need to generate files for MafiaBase mod first...
                    GenerateSolution("Mods/MafiaBase/Temp", "MafiaBase");

                    if (solutionName != "MafiaBase")
                        GenerateSolution(solutionPath, solutionName);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.TextArea("IMPORTANT: Output directory must be contained inside a mod folder, preferably in a folder called \"Temp\". Your mod MUST be located inside of Mods folder, otherwise the generator will fail to work!");
        }

        void GenerateSolution(string path, string name)
        {
            var projectPath = Path.Combine(path, name);

            if (!Directory.Exists(projectPath))
                Directory.CreateDirectory(projectPath);

            var solutionTemplate = File.ReadAllText(SOLUTION_TPL);
            var projectTemplate = File.ReadAllText(PROJECT_TPL);
            string mafiaPath = Directory.GetParent(Application.dataPath).FullName;

            solutionTemplate = solutionTemplate.Replace("[SOLUTION_NAME]", name);
            solutionTemplate = solutionTemplate.Replace("[MAFIA_PATH]", mafiaPath);

            projectTemplate = projectTemplate.Replace("[UNITY_PATH]", unityPath);
            projectTemplate = projectTemplate.Replace("[MAFIA_PATH]", mafiaPath);

            string includeFiles = "";

            var scriptsDir = Path.Combine(path, "..", "Scripts");

            if (Directory.Exists(scriptsDir))
            {
                foreach (var cs in Directory.EnumerateFiles(scriptsDir, "*.cs", SearchOption.AllDirectories))
                {
                    var fcs = cs.Replace(scriptsDir + Path.DirectorySeparatorChar, "");

                    includeFiles += fileInclude.Replace("[INCLUDE_NAME]", fcs);
                }
            }

            projectTemplate = projectTemplate.Replace("[SOURCE_FILES]", includeFiles);

            if (name != "MafiaBase")
            {
                solutionTemplate = solutionTemplate.Replace("[MAFIABASE_STUB]", solutionMafiaBase);
                projectTemplate = projectTemplate.Replace("[MAFIABASE_STUB]", projectMafiaBase);
            }
            else 
            {
                solutionTemplate = solutionTemplate.Replace("[MAFIABASE_STUB]", "");
                projectTemplate = projectTemplate.Replace("[MAFIABASE_STUB]", "");
            }

            File.WriteAllText(Path.Combine(path, name + ".sln"), solutionTemplate);
            File.WriteAllText(Path.Combine(projectPath, name + ".csproj"), projectTemplate);
        }

        const string SOLUTION_TPL = @"Assets/Resources/Solution.tpl";
        const string PROJECT_TPL = @"Assets/Resources/Project.tpl";

        string unityPath = @"F:/Unity/2018.2.2f1";
        string solutionPath = @"Mods/ExampleMod/Temp";
        string solutionName = @"ExampleMod";

        string fileInclude = "<Compile Include=\"..\\..\\Scripts\\[INCLUDE_NAME]\" Link=\"[INCLUDE_NAME]\" />";
        string solutionMafiaBase = "Project(\"{9A19103F-16F7-4668-BE54-9A1E7A4F7556}\") = \"MafiaBase\", \"..\\..\\MafiaBase\\Temp\\MafiaBase\\MafiaBase.csproj\", \"{021E1AD1-69A2-4B96-9B81-1CB1B91C0C47}\"\nEndProject";
        string projectMafiaBase = "<ProjectReference Include=\"..\\..\\..\\MafiaBase\\Temp\\MafiaBase\\MafiaBase.csproj\" />";
    }
    
    [Serializable]
    public class MafiaObjectInjector : EditorWindow
    {
        const string INJECTOR_CFG = "Assets/External Assets/injector.json";

        public List<ObjectInjector> injectors = new List<ObjectInjector>();

        [SerializeField] private int selectedInjector;
        [SerializeField] private string missionName;
        [SerializeField] private string objectName;

        public static void Init()
        {
            var window = GetWindow<MafiaObjectInjector>();
            window.titleContent = new GUIContent("Object Injector");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save Profile"))
                {
                    var jsonContent = JsonUtility.ToJson(this);

                    File.WriteAllText(INJECTOR_CFG, jsonContent);
                }

                if (GUILayout.Button("Load Profile"))
                {
                    var jsonContent = File.ReadAllText(INJECTOR_CFG);

                    JsonUtility.FromJsonOverwrite(jsonContent, this);
                }
            }
            EditorGUILayout.EndHorizontal();

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

                if (editorTask == null)
                {
                    return;
                }

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