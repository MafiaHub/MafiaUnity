using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    public class ObjectDefinition : MonoBehaviour
    {

        public MafiaFormats.Scene2BINLoader.Object data;

        public void Init()
        {
            switch (data.type)
            {
                case MafiaFormats.Scene2BINLoader.ObjectType.Model:
                {
                    // TODO: Load model over here?
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

                    if (data.lightType != MafiaFormats.Scene2BINLoader.LightType.Point)
                        break;

                    var light = gameObject.AddComponent<Light>();

                    light.type = LightType.Point;

                    light.intensity = data.lightPower;
                    light.color = new Color(data.lightColour.x, data.lightColour.y, data.lightColour.z);
                }
                break;
            }
        }
    }
}