using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    public class ObjectDefinition : MonoBehaviour
    {

        [SerializeField] public MafiaFormats.Scene2BINLoader.Object data;

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
                        GameObject.DestroyImmediate(light);
                        break;
                    }

                    if (data.lightType == MafiaFormats.Scene2BINLoader.LightType.Directional)
                    {
                        light.type = LightType.Directional;
                        
                        if (data.lightFlags.HasFlag(MafiaFormats.Scene2BINLoader.LightFlags.LightmapShadows))
                        {
                            //light.shadows = LightShadows.None;
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
    }
}