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
            try
            {
                // Fix backdrop sector
                var backdrop = GameObject.Find("Backdrop sector");
                {
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

                switch (missionName.ToLower())
                {
                    case "00menu":
                    {
                        var projectorRay = GameObject.Find("9promitac/Cylinder18");
                        var projectorIDontCarePart = GameObject.Find("9promitac/Cylinder05");

                        if (projectorRay != null && projectorIDontCarePart != null)
                        {
                            var meshRenderer = projectorRay.GetComponent<MeshRenderer>();
                            var mat = meshRenderer.sharedMaterial;

                            mat.shader = Shader.Find("Unlit/Transparent");
                            var oldTex = mat.GetTexture("_MainTex");
                            var tex = ModelGenerator.LoadTexture(oldTex.name, false, true, true);
                            mat.SetTexture("_MainTex", tex);

                            projectorIDontCarePart.SetActive(false);
                        }

                        var lampRay = GameObject.Find("svetlo");

                        if (lampRay != null)
                        {
                            var meshRenderer = lampRay.GetComponent<MeshRenderer>();
                            var mat = meshRenderer.sharedMaterial;

                            mat.shader = Shader.Find("Unlit/Transparent");
                            var oldTex = mat.GetTexture("_MainTex");
                            var tex = ModelGenerator.LoadTexture(oldTex.name, false, true, true);
                            mat.SetTexture("_MainTex", tex);
                            //mat.SetColor("_Color", new Color(1f, 244/255f, 112/255f, 62/255f));
                        }

                        var photoFrame = GameObject.Find("foto");

                        if (photoFrame != null)
                        {
                            var s = photoFrame.transform.localScale;
                            photoFrame.transform.localScale = new Vector3(2.304092f, s.y, s.z);
                        }
    /* 
                        var menugl = GameObject.Find("menugl/Rectangle04");

                        if (menugl != null)
                        {
                            var meshRenderer = menugl.GetComponent<MeshRenderer>();
                            var mat = meshRenderer.sharedMaterial;

                            mat.shader = Shader.Find("Mafia/Unlit Transparent");
                            mat.SetColor("_Color", new Color(1f, 252 / 255f, 218 / 255f, 2 / 255f));
                        } */
                    }
                    break;

                    case "tutorial":
                    {
                        var skybox = GameObject.Find("o_m_");

                        if (skybox != null)
                        {
                            SetUpSkybox(skybox.transform.Find("Box02"));
                            SetUpSkybox(skybox.transform.Find("Box03"));

                            var slunko = skybox.transform.Find("slunko");
                            slunko.gameObject.SetActive(false);
                        }

                        var light22 = GameObject.Find("sector Box12/light22")?.GetComponent<Light>();
                        if (light22 != null) light22.shadows = LightShadows.Soft;

                        var light10 = GameObject.Find("sector Box12/light10")?.GetComponent<Light>();
                        if (light10 != null) light10.shadows = LightShadows.Soft;

                        var fog = GameObject.Find("light5")?.GetComponent<ObjectDefinition>();
                        SetUpFog(fog);
                    }
                    break;

                    case "mise04-motorest":
                    {
                        var box01 = GameObject.Find("denjasno/Box01");
                        var box02 = GameObject.Find("denjasno/Box02");

                        if (box01 != null)
                            SetUpSkybox(box01.transform);

                        if (box02 != null)
                            SetUpSkybox(box02.transform);
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

                    case "mise20-galery":
                    {
                        var obloha = GameObject.Find("obloha");
                        var obloha01 = GameObject.Find("obloha01");

                        if (obloha != null)
                        {
                            SetUpSkybox(obloha.transform);
                            obloha.transform.parent = backdrop.transform;

                            obloha.transform.localScale = new Vector3(2, 2, 2);
                        }

                        if (obloha01 != null)
                        {
                            obloha01.SetActive(false);
                        }
                    }
                    break;

                    case "freeride":
                    {
                        var box02 = GameObject.Find("zapad/Box02");

                        SetUpSkybox(box02.transform);
                    }
                    break;

                    case "freekrajina":
                    {
                        var box01 = GameObject.Find("denjasno00/Box01");
                        var box02 = GameObject.Find("denjasno00/Box02");

                        SetUpSkybox(box01.transform);
                        SetUpSkybox(box02.transform);
                    }
                    break;


                    case "mise16-letiste":
                    {
                        var box02 = GameObject.Find("denjasno/Box02");
                        var box03 = GameObject.Find("denjasno/Box03");

                        SetUpSkybox(box02.transform);
                        SetUpSkybox(box03.transform);

                        var slunko = GameObject.Find("denjasno/slunko");
                        slunko.gameObject.SetActive(false);
                    }
                    break;
                }


                // NOTE: Create a spawn point for player
                // TODO: Improve it
                /* var player = GameObject.Find("Main Player");

                if (player != null)
                {
                    Debug.Log("Player was found! Locating spawn point.");

                    var objects = Resources.FindObjectsOfTypeAll(typeof(ObjectDefinition));

                    foreach (ObjectDefinition obj in objects)
                    {
                        if (obj.data.specialType == MafiaFormats.Scene2BINLoader.SpecialObjectType.Player)
                        {
                            var tr = obj.transform;

                            player.transform.position = tr.position;
                            player.transform.rotation = tr.rotation;
                            tr.gameObject.SetActive(false);

                            var rg = player.GetComponent<Rigidbody>();
                            rg.velocity = new Vector3(0f, 0f, 0f);
                            rg.angularVelocity = new Vector3(0f, 0f, 0f);
                            break;
                        }
                    }
                } */
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("There was an issue applying patches to the mission. Make sure the loaded mission wasn't edited!\n{0}", ex.ToString());
            }
        }

        void SetUpSkybox(Transform skybox)
        {
            skybox.gameObject.layer = LayerMask.NameToLayer("Backdrop");

            var meshRenderer = skybox.GetComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            foreach (var mat in meshRenderer.sharedMaterials)
            {
                mat.shader = Shader.Find("Unlit/Texture");
            }
        }

        void SetUpFog(ObjectDefinition obj)
        {
            if (obj == null) return;
            var data = obj.data;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(data.lightColour.x, data.lightColour.y, data.lightColour.z, 1f) * data.lightPower;
            RenderSettings.fogStartDistance = data.lightNear * 1000f;
            RenderSettings.fogEndDistance = data.lightFar * 50f;
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
            cam.cullingMask = (1 << LayerMask.NameToLayer("Backdrop"));
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
            if (skyboxCamera != null)
                GameObject.Destroy(skyboxCamera.gameObject);
        }
    }
}