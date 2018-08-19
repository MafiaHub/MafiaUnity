using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    [Serializable]
    public class MissionManager
    {

        [Serializable]
        public class Mission
        {
            public string missionName;
            public GameObject rootObject;
            public MafiaFormats.Scene2BINLoader missionData;
        }

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

            gameAPI.modelGenerator.LoadObject(missionPath + "scene.4ds").transform.parent = missionObject.transform;

            if (gameAPI.fileSystem.Exists(missionPath + "cache.bin"))
                gameAPI.cityGenerator.LoadObject(missionPath + "cache.bin").transform.parent = missionObject.transform;

            gameAPI.sceneGenerator.LoadObject(missionPath + "scene2.bin").transform.parent = missionObject.transform;

            if (gameAPI.fileSystem.Exists(missionPath + "tree.klz"))
                gameAPI.cityGenerator.LoadCollisions(missionPath + "tree.klz").transform.parent = missionObject.transform;

            new MissionHacks(missionName, gameAPI.sceneGenerator.lastLoader);

            mission = new Mission
            {
                missionName = missionName,
                rootObject = missionObject,
                missionData = gameAPI.sceneGenerator.lastLoader
            };

            if (onMissionLoaded != null)
                onMissionLoaded.Invoke(missionName);
        }

        public void DestroyMission()
        {
            if (mission != null)
            {
                GameObject.DestroyImmediate(mission.rootObject, true);

                mission = null;

                if (onMissionDestroyed != null)
                    onMissionDestroyed.Invoke(mission.missionName);
            }

            Resources.UnloadUnusedAssets();
        }

    }
}