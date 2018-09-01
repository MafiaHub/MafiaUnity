using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MafiaUnity
{
#if UNITY_EDITOR
    public class MafiaUtils : ScriptableWizard
    {

        [MenuItem("Mafia SDK/Open Editor")]
        static void SpawnDevObject()
        {
            var mafiaEditor = GetEditorHandle<MafiaEditor>();
            mafiaEditor.Init();
        }

        [MenuItem("Mafia SDK/Mod Manager")]
        static void SpawnModManager()
        {
            var mafiaModManager = GetEditorHandle<MafiaModManager>();
            mafiaModManager.Init();
        }

        [MenuItem("Mafia SDK/Object Injector")]
        static void SpawnObjectInjector()
        {
            var mafiaObjectInjector = GetEditorHandle<MafiaObjectInjector>();
            mafiaObjectInjector.Init();
        }

        [MenuItem("Mafia SDK/Solution Generator")]
        static void SpawnSolutionGenerator()
        {
            var mafiaGenerateSolution = GetEditorHandle<MafiaGenerateSolution>();
            mafiaGenerateSolution.Init();
        }

        private static T GetEditorHandle<T>() where T : EditorWindow
        {
            T hwnd = (T)ScriptableObject.CreateInstance(typeof(T).ToString());
            return hwnd;
        }
    }
#endif
}
