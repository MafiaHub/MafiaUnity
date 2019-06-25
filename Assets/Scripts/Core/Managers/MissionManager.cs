using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    [Serializable]
    public class Mission
    {
        public string missionName;
        public GameObject rootObject;
        public MafiaFormats.Scene2BINLoader missionData;
        public Dictionary<string, GameObject> referenceMap;
        public Dictionary<string, GameObject> cacheReferenceMap;
    }

    [Serializable]
    public class MissionManager
    {
        [SerializeField] public Mission mission { get; private set; }
        
        public delegate void OnMissionLoading(string missionName);
        public OnMissionLoading onMissionLoading;

        public delegate void OnMissionLoaded(string missionName);
        public OnMissionLoaded onMissionLoaded;

        public delegate void OnMissionDestroyed(string missionName);
        public OnMissionDestroyed onMissionDestroyed;

        public void LoadMission(string missionName, bool ignoreExistingMission=false)
        {
            if (!ignoreExistingMission)
                DestroyMission();

            if (onMissionLoading != null)
                onMissionLoading.Invoke(missionName);

            GameObject missionObject = new GameObject(missionName);

            var gameAPI = GameAPI.instance;

            var missionPath = "missions/" + missionName + "/";

            mission = new Mission
            {
                missionName = missionName,
                rootObject = missionObject,
                referenceMap = new Dictionary<string, GameObject>(),
                cacheReferenceMap = new Dictionary<string, GameObject>(),
            };

            gameAPI.modelGenerator.LoadObject(missionPath + "scene.4ds", mission).transform.parent = missionObject.transform;

            if (gameAPI.fileSystem.Exists(missionPath + "cache.bin"))
                gameAPI.cityGenerator.LoadObject(missionPath + "cache.bin", mission).transform.parent = missionObject.transform;

            gameAPI.sceneGenerator.LoadObject(missionPath + "scene2.bin", mission).transform.parent = missionObject.transform;

            if (gameAPI.fileSystem.Exists(missionPath + "tree.klz"))
                gameAPI.cityGenerator.LoadCollisions(mission, missionPath + "tree.klz").transform.parent = missionObject.transform;

            mission.missionData = gameAPI.sceneGenerator.lastLoader;

            new MissionHacks(missionName, gameAPI.sceneGenerator.lastLoader);

            if (onMissionLoaded != null)
                onMissionLoaded.Invoke(missionName);
        }

        public void DestroyMission()
        {
            if (mission != null)
            {
                Debug.LogFormat("Releasing resources from an older mission: {0}", mission.missionName);
                GameObject.DestroyImmediate(mission.rootObject, true);

                ObjectDefinition.ResetLightCache();

                if (onMissionDestroyed != null)
                    onMissionDestroyed.Invoke(mission.missionName);

                mission = null;
            }

            Resources.UnloadUnusedAssets();
        }

    }
}