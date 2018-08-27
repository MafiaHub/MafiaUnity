using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    public class ObjectDefinition : MonoBehaviour
    {

        [SerializeField] public MafiaFormats.Scene2BINLoader.Object data;
        public static List<ObjectDefinition> fogLights = new List<ObjectDefinition>();
        public static List<ObjectDefinition> ambientLights = new List<ObjectDefinition>();

        public Bounds sectorBounds;

        private static GameObject mainPlayer = null;

        public static void ResetLightCache()
        {
            fogLights = new List<ObjectDefinition>();
            ambientLights = new List<ObjectDefinition>();
        }

        public void Init()
        {
            switch (data.type)
            {
                case MafiaFormats.Scene2BINLoader.ObjectType.Model:
                {
                    
                }
                break;

                case MafiaFormats.Scene2BINLoader.ObjectType.Sector:
                {
                    
                }
                break;

                case MafiaFormats.Scene2BINLoader.ObjectType.Light:
                {
                    //NOTE(zaklaus): Re-parent the light
                    var parent = BaseGenerator.FetchCacheReference(GameAPI.instance.missionManager.mission, data.lightSectors == null ? "" : data.lightSectors)?.gameObject;

                    if (parent != null)
                    {
                        transform.parent = parent.transform;

                        transform.localPosition = data.pos;
                        transform.localRotation = data.rot;
                        transform.localScale = data.scale;
                    }

                    if (data.lightType != MafiaFormats.Scene2BINLoader.LightType.Spot && data.lightType != MafiaFormats.Scene2BINLoader.LightType.Point
                        && data.lightType != MafiaFormats.Scene2BINLoader.LightType.Directional
                        && data.lightType != MafiaFormats.Scene2BINLoader.LightType.Ambient
                        && data.lightType != MafiaFormats.Scene2BINLoader.LightType.Fog)
                        break;

                    var light = gameObject.AddComponent<Light>();

                    light.type = LightType.Point;
                    light.shadows = LightShadows.Soft;

                    if (data.lightType == MafiaFormats.Scene2BINLoader.LightType.Spot)
                    {
                        light.type = LightType.Spot;
                        light.spotAngle = Mathf.Rad2Deg * data.lightAngle;
                    }

                    if (data.lightType == MafiaFormats.Scene2BINLoader.LightType.Fog)
                    {
                        light.intensity = 0f;
                        light.color = Color.blue;
                        light.type = LightType.Area; // Just to distinguish it easily
                        light.shadows = LightShadows.None;
                        break;
                    }

                    if (data.lightType == MafiaFormats.Scene2BINLoader.LightType.Ambient)
                    {
                        light.intensity = 0f;
                        light.color = Color.red;
                        light.type = LightType.Area; // Just to distinguish it easily
                        light.shadows = LightShadows.None;
                        break;
                    }

                    if (data.lightType == MafiaFormats.Scene2BINLoader.LightType.Directional)
                    {
                        light.type = LightType.Directional;
                        //Debug.Log(gameObject.name + ": " + (int)data.lightFlags);
                        
                        // TODO
                        if ((int)data.lightFlags != 107)
                        {
                            GameObject.DestroyImmediate(light);
                            break;
                        }
                    }

                    light.intensity = data.lightPower;
                    light.range = data.lightFar;
                    light.color = new Color(data.lightColour.x, data.lightColour.y, data.lightColour.z);
                }
                break;
            }

            switch (data.specialType)
            {
                case MafiaFormats.Scene2BINLoader.SpecialObjectType.Physical:
                {

                }
                break;
                
                case MafiaFormats.Scene2BINLoader.SpecialObjectType.Door:
                {
                    var meshFilter = GetComponent<MeshFilter>();

                    if (meshFilter != null)
                    {
                        gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;

                        var door = gameObject.AddComponent<Door>();
                        door.door = data.doorObject;
                    }
                }
                break;
            }
        }

        public void Start()
        {
            switch (data.type)
            {
                case MafiaFormats.Scene2BINLoader.ObjectType.Sector:
                {
                    if (sectorBounds.size.magnitude == 0f)
                        sectorBounds = GetMaxBounds();
                }
                break;

                case MafiaFormats.Scene2BINLoader.ObjectType.Light:
                {
                    switch (data.lightType)
                    {
                        case MafiaFormats.Scene2BINLoader.LightType.Fog:
                        {
                            fogLights.Add(this);
                        }
                        break;
                        
                        case MafiaFormats.Scene2BINLoader.LightType.Ambient:
                        {
                            ambientLights.Add(this);
                        }
                        break;
                        
                    }
                }
                break;
            }
        }

        /* public void FixedUpdate()
        {
            if (mainPlayer == null)
                mainPlayer = GameObject.Find("Main Player");

            switch (data.type)
            {

            }
        } */

        Bounds GetMaxBounds()
        {
            var b = new Bounds(transform.position, Vector3.zero);

            foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
            {
                b.Encapsulate(r.bounds);
            }

            return b;
        }
    }
}