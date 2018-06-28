using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace OpenMafia
{
    public class MafiaPanel : MonoBehaviour
    {
        
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MafiaPanel))]
    public class MafiaPanelEditor : Editor
    {
        public string gamePath = "F:/Games/Mafia/";
        public string modelPath = "missions/freeride/scene.4ds";
        public string cityPath = "missions/freeride/cache.bin";
        public string missionName = "freeride";

        bool isInitialized = false;


        public override void OnInspectorGUI()
        {
            if (!isInitialized && GameManager.instance.cvarManager.values != null)
            {
                gamePath = GameManager.instance.cvarManager.Get("gamePath", gamePath);
                missionName = GameManager.instance.cvarManager.Get("editorMissionName", missionName);
                isInitialized = true;
            }

            EditorGUILayout.PrefixLabel("Game Path");
            gamePath = EditorGUILayout.TextField(gamePath);

            if (GUILayout.Button("Set Path"))
            {
                GameManager.instance.SetGamePath(gamePath);
            }

            GUILayout.Space(15);

            modelPath = EditorGUILayout.TextField(modelPath);

            if (GUILayout.Button("Spawn Object"))
            {
                if (GameManager.instance.modelGenerator.LoadObject(modelPath) == null)
                    Debug.LogWarning("Model couldn't be spawned! My path is " + GameManager.instance.gamePath);
            }

            cityPath = EditorGUILayout.TextField(cityPath);

            if (GUILayout.Button("Spawn City"))
            {
                if (GameManager.instance.cityGenerator.LoadObject(cityPath) == null)
                    Debug.LogWarning("City couldn't be spawned! My path is " + GameManager.instance.gamePath);
            }

            missionName = EditorGUILayout.TextField(missionName);

            if (GUILayout.Button("Spawn Mission"))
            {
                GameManager.instance.missionManager.LoadMission(missionName);
            }

            if (GUILayout.Button("Destroy Mission"))
            {
                GameManager.instance.missionManager.DestroyMission();
            }

            if (GUILayout.Button("Spawn Scene Only"))
            {
                GameManager.instance.sceneGenerator.LoadObject("missions/" + missionName + "/scene2.bin");
            }


            GUILayout.Space(15);

            if (GUILayout.Button("Save Config"))
            {
                var cvars = GameManager.instance.cvarManager;
                cvars.ForceSet("gamePath", gamePath, CvarManager.CvarMode.Archived);
                cvars.ForceSet("editorMissionName", missionName, CvarManager.CvarMode.Archived);
                cvars.SaveMainConfig();
            }

            if (GUILayout.Button("Fix Scene View Distance"))
            {
                SceneView.lastActiveSceneView.camera.farClipPlane = 50;
            }
        }
    }

    public class MafiaPanelWizard : ScriptableWizard
    {

    }

#endif
}