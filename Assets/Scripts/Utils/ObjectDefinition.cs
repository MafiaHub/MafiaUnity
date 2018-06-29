using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenMafia
{
    public class ObjectDefinition : MonoBehaviour
    {

        public MafiaFormats.Scene2BINLoader.Object data;

        public void Init()
        {
            switch (data.type)
            {
                case MafiaFormats.Scene2BINLoader.ObjectType.OBJECT_TYPE_MODEL:
                {
                    // TODO: Load model over here?
                }
                break;

                case MafiaFormats.Scene2BINLoader.ObjectType.OBJECT_TYPE_LIGHT:
                {
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