using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OpenMafia
{
    public class MafiaPanel : MonoBehaviour
    {
        
    }

    [CustomEditor(typeof(MafiaPanel))]
    public class MafiaPanelEditor : Editor
    {
        public string gamePath = "D:/Mafia 1.2/";
        public string modelPath = "missions/freekrajina/scene.4ds";

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PrefixLabel("Game Path");
            gamePath = EditorGUILayout.TextField(gamePath);

            if (GUILayout.Button("Set Path"))
            {
                GameLoader.instance.SetGamePath(gamePath);
            }

            GUILayout.Space(15);

            modelPath = EditorGUILayout.TextField(modelPath);

            if (GUILayout.Button("Spawn Object"))
            {
                if (GameLoader.instance.modelLoader.LoadModel(modelPath) == null)
                    Debug.LogWarning("Model couldn't be spawned! My path is " + GameLoader.instance.gamePath);
            }
        }
    }

    public class MafiaPanelWizard : ScriptableWizard
    {

    }
}