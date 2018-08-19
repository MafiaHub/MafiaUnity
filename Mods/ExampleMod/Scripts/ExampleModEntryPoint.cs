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

        var go = new GameObject("ExampleModObject");
        go.AddComponent<ExampleMonoBehaviourScript>();
    
        var tommy = GameAPI.instance.modelGenerator.LoadObject("models/Tommy.4ds");
        
        var player = tommy.AddComponent<ModelAnimationPlayer>();
        player.LoadAndSetAnimation("anims/!!!Skakani.5ds");
        player.isPlaying = true;
        player.playbackMode = ModelAnimationPlayer.AnimationPlaybackMode.Repeat;

        var terrainBundle = mod.LoadFromFile("sampleterrain");

        if (terrainBundle != null)
        {
            Debug.Log("AssetBundle has been loaded!");

            var terrainPrefab = terrainBundle.LoadAsset<GameObject>("Terrain");
            GameObject.Instantiate(terrainPrefab);
        }
    }
}