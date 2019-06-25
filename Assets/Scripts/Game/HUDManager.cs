using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using MafiaUnity;

public class HUDManager 
{
    #region Singleton

    private static HUDManager _instance = null;
    public static HUDManager instance { get {if (_instance == null) _instance = new HUDManager(); return _instance; } }

    #endregion

    class Entry
    {
        public Rect region;
        public Texture2D sprite;
    }

    class UIAtlas
    {
        public Texture2D atlas;
    }

    Dictionary<string, UIAtlas> atlases = new Dictionary<string, UIAtlas>();
    Dictionary<string, Entry> entries = new Dictionary<string, Entry>();

    bool isNumbersFontLoaded = false;

    /// <summary>
    /// UI scale, which can be configured externally.
    /// </summary>
    public float scale = 1f;

    /// <summary>
    /// Loads specified atlas into memory.
    /// </summary>
    /// <param name="name">Raw file name of an atlas. It gets converted into 'maps/[name].tga'</param>
    /// <param name="blackIsTransparency">Specifies whether to use black color as a transparency or rely on the alpha channel</param>
    public void LoadAtlas(string name, bool blackIsTransparency=false)
    {
        if (atlases.ContainsKey(name))
            return;

        var atlas = new UIAtlas();
        var stream = GameAPI.instance.fileSystem.GetStreamFromPath(Path.Combine("maps", name + ".tga"));

        if (stream == null)
        {
            Debug.LogWarning(string.Format("Atlas {0} was not found!", name));
            return;
        }

        atlas.atlas = TGALoader.LoadTGA(stream, blackIsTransparency);

        atlases.Add(name, atlas);
    }

    /// <summary>
    /// Loads a sprite from atlas into memory. Requires atlas to be loaded first.
    /// </summary>
    /// <param name="name">Sprite name</param>
    /// <param name="atlasName">UI atlas name to crop sprite from</param>
    /// <param name="region">Atlas region to crop</param>
    public void LoadSprite(string name, string atlasName, Rect region)
    {
        if (entries.ContainsKey(name))
            return;

        if (!atlases.ContainsKey(atlasName))
        {
            Debug.LogWarningFormat("Could not load sprite {0}, UI atlas {1} not found.", name, atlasName);
            return;
        }

        var entry = new Entry();
        entry.region = region;
        entry.sprite = atlases[atlasName].atlas.CropTexture(region);

        entries.Add(name, entry);
    }

    /// <summary>
    /// Loads 1numbers.tga atlas used for drawing numbers on the screen.
    /// </summary>
    public void LoadNumbersFont()
    {
        if (isNumbersFontLoaded)
            return;

        LoadAtlas("1numbers");
        
        LoadSprite("num/", "1numbers", new Rect(110, 0, 11, 16));

        for (int i = 0; i < 10; i++)
        {
            LoadSprite("num" + i.ToString(), "1numbers", new Rect(i*11, 0, 11, 16));
        }


        isNumbersFontLoaded = true;
    }

    public Rect ProcessRelativePosition(HUDAnchorMode mode, Vector2 offset, Texture2D sprite)
    {
        var region = new Rect();
        if (sprite == null) return region;

        if (mode.HasFlag(HUDAnchorMode.Right))
        {
            region.x = Screen.width - sprite.width * scale - offset.x;
        }
        else
        {
            region.x = offset.x;
        }

        if (mode.HasFlag(HUDAnchorMode.Bottom))
        {
            region.y = Screen.height - sprite.height * scale - offset.y;
        }
        else
        {
            region.y = offset.y;
        }

        if (mode.HasFlag(HUDAnchorMode.MiddleX))
        {
            region.x = Screen.width / 2f - sprite.width * scale / 2f - offset.x;
        }

        if (mode.HasFlag(HUDAnchorMode.MiddleY))
        {
            region.y = Screen.height / 2f - sprite.height * scale / 2f - offset.y;
        }

        region.width = sprite.width * scale;
        region.height = sprite.height * scale;

        return region;
    }

    public void DrawNumber(int num, HUDAnchorMode mode, Vector2 offset, float numScale=1f)
    {
        if (!isNumbersFontLoaded)
            LoadNumbersFont();

        char[] buf = num.ToString().ToCharArray();

        var region = ProcessRelativePosition(mode, offset*numScale, GetSprite("num1"));
        region.width  *= numScale;
        region.height *= numScale;

        foreach (var c in buf)
        {
            var spriteName = "num" + c;

            region.x += 11f*scale*numScale;

            GUI.DrawTexture(region, entries[spriteName].sprite);
        }
    }

    public void DrawSpriteAbsoluteScaled(string name, Vector2 pos)
    {
        if (!entries.ContainsKey(name))
            return;

        var entry = entries[name];

        GUI.DrawTexture(new Rect(pos.x, pos.y, entry.sprite.width*scale, entry.sprite.height*scale), entry.sprite);
    }

    public void DrawSpriteAbsoluteScaled(string name, Rect region)
    {
        if (!entries.ContainsKey(name))
            return;

        var entry = entries[name];

        region.width *= scale;
        region.height *= scale;

        GUI.DrawTexture(region, entry.sprite);
    }

    public void DrawSpriteAbsolute(string name, Vector2 pos)
    {
        if (!entries.ContainsKey(name))
            return;

        var entry = entries[name];

        GUI.DrawTexture(new Rect(pos.x, pos.y, entry.sprite.width, entry.sprite.height), entry.sprite);
    }

    public void DrawSpriteAbsolute(string name, Rect region)
    {
        if (!entries.ContainsKey(name))
            return;

        var entry = entries[name];

        GUI.DrawTexture(region, entry.sprite);
    }

    public void DrawSprite(string name, HUDAnchorMode mode, Vector2 offset)
    {
        if (!entries.ContainsKey(name))
            return;

        var entry = entries[name];
        var sprite = entry.sprite;
        var region = ProcessRelativePosition(mode, offset, sprite);

        GUI.DrawTexture(region, entry.sprite);
    }

    public Texture2D GetSprite(string name)
    {
        if (!entries.ContainsKey(name))
            return null;

        return entries[name].sprite;
    }
}

public enum HUDAnchorMode : uint
{
    None = 0x0,
    Right = 0x1,
    Bottom = 0x2,
    MiddleX = 0x4,
    MiddleY = 0x8,
}
