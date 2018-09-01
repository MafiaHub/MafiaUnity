using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using MafiaUnity;

class ScriptMain : IModScript
{
    void IModScript.Start(Mod mod)
    {
        Debug.Log("ExampleMod was initialized! Version: " + mod.version);
        GameAPI.instance.skipLoadingMainGame = true; /* avoids loading base game, effectively overriding the startup sequence. */
        GameAPI.instance.blockMods = true; /* prevents other mods from loading. This is example mod after all, we don't want any aliens here, do we? */
        GameAPI.instance.avoidLooseFiles = true; /* avoid loading any loose files in the game directory. We want a vanilla experience. */

        var cam = Camera.main;

        cam.transform.position = new Vector3(0.58f, 8.41f, 2.76f);
        cam.transform.rotation = Quaternion.Euler(new Vector3(90f, 0, 0));
        cam.farClipPlane = 5000f;

        var go = new GameObject("ExampleModObject");
        go.AddComponent<ExampleMonoBehaviourScript>();
    
        var tommy = GameAPI.instance.modelGenerator.LoadObject("models/Tommy.4ds", null);
        tommy.transform.position = new Vector3(0.5f, 0, 0);
        
        var player = tommy.AddComponent<ModelAnimationPlayer>();
        player.LoadAndSetAnimation("anims/!!!Skakani.5ds");
        player.isPlaying = true;
        player.playbackMode = ModelAnimationPlayer.AnimationPlaybackMode.Repeat;

        var terrainBundle = mod.LoadFromFile("sampleterrain");

        if (terrainBundle != null)
        {
            Debug.Log("AssetBundle has been loaded!");

            var terrainPrefab = terrainBundle.LoadAsset<GameObject>("Terrain");
            var cl = GameObject.Instantiate(terrainPrefab);

            cl.transform.position = new Vector3(-308f, -115.8f, -213.8f);

            var sun = new GameObject("slnko").AddComponent<Light>();
            sun.transform.rotation = Quaternion.Euler(new Vector3(4.994f, 56.028f, 123.647f));
            sun.shadows = LightShadows.Soft;
            sun.type = LightType.Directional;
        }
    }
}