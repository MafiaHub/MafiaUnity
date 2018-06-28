using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenMafia
{
    public class MissionManager
    {

        public class Mission
        {
            public string missionName;
            public GameObject rootObject;
            public MafiaFormats.Scene2BINLoader missionData;
        }

        public Mission mission { get; private set; }
        
        public delegate void OnMissionLoading(string missionName);
        public OnMissionLoading onMissionLoading;

        public delegate void OnMissionLoaded(string missionName);
        public OnMissionLoaded onMissionLoaded;

        public delegate void OnMissionDestroyed(string missionName);
        public OnMissionDestroyed onMissionDestroyed;

        public void LoadMission(string missionName)
        {
            DestroyMission();

            if (onMissionLoading != null)
                onMissionLoading.Invoke(missionName);

            GameObject missionObject = new GameObject(missionName);

            var gameManager = GameManager.instance;

            var missionPath = "missions/" + missionName + "/";
            
            gameManager.sceneGenerator.LoadObject(missionPath + "scene2.bin").transform.parent = missionObject.transform;
            gameManager.modelGenerator.LoadObject(missionPath + "scene.4ds").transform.parent = missionObject.transform;

            if (File.Exists(gameManager.gamePath + missionPath + "cache.bin"))
                gameManager.cityGenerator.LoadObject(missionPath + "cache.bin").transform.parent = missionObject.transform;

            mission = new Mission
            {
                missionName = missionName,
                rootObject = missionObject,
                missionData = gameManager.sceneGenerator.lastLoader
            };

            if (onMissionLoaded != null)
                onMissionLoaded.Invoke(missionName);
        }

        public void DestroyMission()
        {
            if (mission != null)
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(mission.rootObject);
#else
                GameObject.Destroy(mission.rootObject);
#endif

                mission = null;

                if (onMissionDestroyed != null)
                    onMissionDestroyed.Invoke(mission.missionName);
            }

        }

    }
}