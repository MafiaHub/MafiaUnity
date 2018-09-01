using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MafiaUnity
{
#if UNITY_EDITOR
    [Serializable]
    public class MafiaGenerateSolution : EditorWindow
    {
        public void Init()
        {
            titleContent = new GUIContent("Solution Generation");
            Show();
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

            File.WriteAllText(Path.Combine(path, name + ".sln"), solutionTemplate);
            File.WriteAllText(Path.Combine(projectPath, name + ".csproj"), projectTemplate);
        }

        const string SOLUTION_TPL = @"Assets/Resources/Solution.tpl";
        const string PROJECT_TPL = @"Assets/Resources/Project.tpl";

        string unityPath = @"F:/Unity/2018.2.6f1";
        string solutionPath = @"Mods/ExampleMod/Temp";
        string solutionName = @"ExampleMod";

        string fileInclude = "<Compile Include=\"..\\..\\Scripts\\[INCLUDE_NAME]\" Link=\"[INCLUDE_NAME]\" />";
    }
#endif
}