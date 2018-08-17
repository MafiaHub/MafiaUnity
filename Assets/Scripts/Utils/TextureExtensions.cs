using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MafiaUnity;

namespace MafiaUnity
{
    public static class Texture2DExtension
    {
        public static Texture2D CropTexture(this Texture2D texture, Rect region)
        {
            Texture2D dest = new Texture2D((int)(region.width), (int)(region.height), texture.format, false);

            var destPixels = texture.GetPixels((int)region.x, (int)(texture.height - region.height) - (int)region.y, (int)region.width, (int)region.height);

            dest.SetPixels(destPixels);
            dest.Apply();

            return dest;
        }
    }
}