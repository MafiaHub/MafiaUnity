using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OpenMafia
{
#if UNITY_EDITOR
    public class MafiaUtils : ScriptableWizard
    {

        [MenuItem("Mafia SDK/Open Editor")]
        static void SpawnDevObject()
        {
            MafiaEditor.Init();
        }
    }
#endif
}
