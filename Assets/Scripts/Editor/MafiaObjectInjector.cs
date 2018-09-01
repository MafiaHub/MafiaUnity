using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MafiaUnity
{
#if UNITY_EDITOR
    [Serializable]
    public class MafiaObjectInjector : EditorWindow
    {
        const string INJECTOR_CFG = "Assets/External Assets/injector.json";

        public List<ObjectInjector> injectors = new List<ObjectInjector>();

        [SerializeField] private int selectedInjector;
        [SerializeField] private string missionName;
        [SerializeField] private string objectName;

        public void Init()
        {
            titleContent = new GUIContent("Object Injector");
            Show();
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
                    injectors.Add(new ObjectInjector(GameAPI.instance.missionManager.mission.missionName));
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
                injector.InjectMissionLoaded(GameAPI.instance.missionManager.mission.missionName);
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