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
        public MissionHacks(string missionName, MafiaFormats.Scene2BINLoader data)
        {
            switch (missionName)
            {
                // example usage
                case "tutorial":
                {
                    var skybox = GameObject.Find("o_m_");

                    if (skybox != null)
                    {
                        SetUpSkybox(skybox.transform.Find("Box02"));
                        SetUpSkybox(skybox.transform.Find("Box03"));
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

                case "mise06-autodrom":
                {
                    var box01 = GameObject.Find("oblohamirrored/Box01");
                    var box02 = GameObject.Find("denjasno/Box02");

                    if (box01 != null)
                        SetUpSkybox(box01.transform);

                    if (box02 != null)
                        SetUpSkybox(box02.transform);
                }
                break;
            }

            // Fix backdrop sector
            {
                var backdrop = GameObject.Find("Backdrop sector");

                if (backdrop != null)
                {
                    backdrop.AddComponent<BackdropManipulator>();
                }
            }

            // Change view distance
            {
                var mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

                var viewDistance = data.viewDistance;

                mainCamera.farClipPlane = viewDistance;
            }
        }

        void SetUpSkybox(Transform skybox)
        {
            var meshRenderer = skybox.GetComponent<MeshRenderer>();

            foreach (var mat in meshRenderer.sharedMaterials)
            {
                mat.shader = Shader.Find("Unlit/Texture");
            }
        }
    }

    public class BackdropManipulator : MonoBehaviour
    {
        Transform mainCamera = null;
        Transform skyboxCamera = null;

        private void Start()
        {
            mainCamera = GameObject.Find("Main Camera").transform;
            skyboxCamera = new GameObject("Backdrop camera").transform;

            skyboxCamera.parent = mainCamera;

            skyboxCamera.localPosition = Vector3.zero;
            skyboxCamera.localRotation = Quaternion.identity;
            skyboxCamera.localScale = Vector3.one;

            var cam = skyboxCamera.gameObject.AddComponent<Camera>();
            cam.farClipPlane = 5000f;
            cam.cullingMask = LayerMask.NameToLayer("Backdrop");
            cam.depth = 1 + mainCamera.GetComponent<Camera>().depth;
            cam.clearFlags = CameraClearFlags.Nothing;

            gameObject.layer = LayerMask.NameToLayer("Backdrop");
        }

        private void Update()
        {
            if (mainCamera == null)
                return;

            transform.position = mainCamera.position;
        }

        private void OnDestroy()
        {
            GameObject.DestroyImmediate(skyboxCamera, true);
        }
    }
}