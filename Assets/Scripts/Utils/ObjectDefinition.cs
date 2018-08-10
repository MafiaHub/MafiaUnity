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

                case MafiaFormats.Scene2BINLoader.ObjectType.Light:
                {
                    //NOTE(zaklaus): Re-parent the light
                    var parent = GameObject.Find(data.lightSectors);

                    if (parent != null)
                    {
                        transform.parent = parent.transform;

                        transform.localPosition = data.pos;
                        transform.localRotation = data.rot;
                        transform.localScale = data.scale;
                    }

                    if (data.lightType != MafiaFormats.Scene2BINLoader.LightType.Directional && data.lightType != MafiaFormats.Scene2BINLoader.LightType.Point)
                        break;

                    var light = gameObject.AddComponent<Light>();

                    light.type = LightType.Point;

                    if (data.lightType == MafiaFormats.Scene2BINLoader.LightType.Directional)
                    {
                        light.type = LightType.Spot;
                        light.spotAngle = Mathf.Rad2Deg * data.lightAngle;
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
                    }
                    
                    Debug.Log(data.doorObject.closeSound);
                }
                break;
            }
        }
    }
}