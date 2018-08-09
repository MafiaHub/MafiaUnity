using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    /* 
        This file consists of hacks used to modify game's scene depending on the mission loaded.
    */
    public class MissionHacks
    {
        public MissionHacks(string missionName)
        {
            switch (missionName)
            {
                // example usage
                case "tutorial":
                {
                    var skybox = GameObject.Find("o_m_");

                    if (skybox != null)
                    {
    #if UNITY_EDITOR
                            GameObject.DestroyImmediate(skybox);
    #else
                            GameObject.Destroy(skybox);
    #endif
                    }
                }
                break;
            }
        }
    }
}