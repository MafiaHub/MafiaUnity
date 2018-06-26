using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OpenMafia
{
    public class MafiaUtils : ScriptableWizard
    {

        [MenuItem("MafiaUtils/Spawn Development Suite")]
        static void SpawnDevObject()
        {
            var dev = new GameObject("DEVEL");

            dev.AddComponent<MafiaPanel>();
        }
    }
}
