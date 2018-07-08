using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MafiaUnity
{
    [Serializable]
    public class ObjectInjector
    {
        
        public enum ObjectFindMode
        {
            Equals,
            Contains,
            StartsWith,
            EndsWith
        }

        [Serializable]
        public class ObjectTask
        {
            public string name;
            public ObjectFindMode findMode;
            public Action<GameObject[]> task;
        }

        public List<ObjectTask> tasks;

        public string requestedMissionName;

        public ObjectInjector(string missionName)
        {
            requestedMissionName = missionName;
            GameManager.instance.missionManager.onMissionLoaded += InjectMissionLoaded;
            tasks = new List<ObjectTask>();
        }

        ~ObjectInjector()
        {
            GameManager.instance.missionManager.onMissionLoaded -= InjectMissionLoaded;
        }

        public void InjectMissionLoaded(string missionName)
        {
            if (missionName != requestedMissionName)
                return;

            foreach (var task in tasks)
                task.task?.Invoke(Resources.FindObjectsOfTypeAll<GameObject>().Where(x => FindObject(x.name, task)).ToArray());
        }

        static bool FindObject(string name, ObjectTask task)
        {
            switch (task.findMode)
            {
                case ObjectFindMode.Equals:
                    return name == task.name;

                case ObjectFindMode.Contains:
                    return name.Contains(task.name);

                case ObjectFindMode.StartsWith:
                    return name.StartsWith(task.name);

                case ObjectFindMode.EndsWith:
                    return name.EndsWith(task.name);
            }

            return false;
        }

        public static void TaskDestroy(GameObject[] objects)
        {
            foreach (var obj in objects)
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(obj);
#else
                GameObject.Destroy(obj);
#endif
            }
        }
    }

    /// <summary>
    /// Inherit from this class to specify custom tasks
    /// </summary>
    public class BaseTaskList
    {
        public void TaskExample(GameObject[] objects)
        {
            Debug.Log("This is an example method");
        }
    }
}