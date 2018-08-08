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
            MafiaEditor.Init();
        }

        [MenuItem("Mafia SDK/Mod Manager")]
        static void SpawnModManager()
        {
            MafiaModManager.Init();
        }
    }
#endif
}
