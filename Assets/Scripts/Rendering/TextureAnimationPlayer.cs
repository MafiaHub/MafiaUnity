using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MafiaUnity;

public class TextureAnimationPlayer : MonoBehaviour {

    public List<Texture2D> frames;
    public Material material;
    public uint framePeriod;

    private float timeUntilNextFrame;
    private int frameIndex;

    private void Start()
    {
        ResetTime();
    }

    void Update () {
        if (GameManager.instance.isPaused)
            return;

        if (timeUntilNextFrame < 0)
        {
            ResetTime();

            if (frames.Count > 0)
                material.SetTexture("_MainTex", frames[frameIndex++ % frames.Count]);
        }
        else
            timeUntilNextFrame -= Time.deltaTime;
	}

    void ResetTime()
    {
        timeUntilNextFrame = framePeriod / 1000f;
    }
}
