using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
                        GameObject.DestroyImmediate(skybox, true);
                    }

                    // handle roof
                    {
                        var box05a = GameObject.Find("sector Box05a/Box05a");

                        if (box05a != null)
                        {
                            var meshRenderer = box05a.GetComponent<MeshRenderer>();

                            if (meshRenderer != null)
                            {
                                meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
                            }
                        }

                        var box10 = GameObject.Find("sector Box12/Box12/Box10");

                        if (box10 != null)
                        {
                            var meshRenderer = box10.GetComponent<MeshRenderer>();

                            if (meshRenderer != null)
                            {
                                meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
                            }
                        }
                    }
                }
                break;
            }
        }
    }
}