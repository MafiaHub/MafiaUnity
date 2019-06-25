using MafiaUnity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ModelViewer : MonoBehaviour
{
    private string gamePath = "C:/Games/Mafia";
    private string modelName = "Tommy.4ds";
    private string animName = "!!!Skakani.5ds";
    private string errorString = "";
    private bool repeatAnim = true;
    private GameObject loadedModel = null;
    private ModelAnimationPlayer animPlayer = null;
    private MouseOrbitImproved orbitCam = null;
    
    private void Start()
    {
        var cvars = GameAPI.instance.cvarManager;
        if (cvars.Contains("gamePath"))
        {
            var path = cvars.Get("gamePath", "C:/Games/Mafia");
            Debug.Log("Game path was detected: " + path);

            gamePath = path;
            GameAPI.instance.SetGamePath(gamePath);
        }

        orbitCam = Camera.main.GetComponent<MouseOrbitImproved>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Hold Left Control to orbit the camera, use your mouse wheel to zoom.");
        GUILayout.Label("Game path");
        gamePath = GUILayout.TextField(gamePath);

        GUILayout.Label("Model name (.4ds)");
        modelName = GUILayout.TextField(modelName);

        GUILayout.Label("Animation name (.5ds)");
        animName = GUILayout.TextField(animName);
        
        repeatAnim = GUILayout.Toggle(repeatAnim, "Repeat animation");

        if (animPlayer != null)
            animPlayer.playbackMode = (repeatAnim) ? ModelAnimationPlayer.AnimationPlaybackMode.Repeat : ModelAnimationPlayer.AnimationPlaybackMode.Once;

        if (GUILayout.Button("Load model"))
        {
            LoadModel();
        }


        if (GUILayout.Button("Play animation"))
        {
            PlayAnim();
        }


        if (GUILayout.Button("Stop animation"))
        {
            StopAnim();
        }

        GUILayout.Label(errorString);
    }

    private bool SetPath()
    {
        errorString = "";
        
        if (!GameAPI.instance.SetGamePath(gamePath))
        {
            Debug.Log("Game path is incorrect!");
            errorString = "Game path is incorrect!";
            return false;
        }

        GameAPI.instance.cvarManager.ForceSet("gamePath", gamePath, CvarManager.CvarMode.Archived);
        return true;
    }

    private void LoadModel()
    {
        if (!SetPath()) return;
        errorString = "";

        if (loadedModel != null)
        {
            GameObject.DestroyImmediate(loadedModel);
            animPlayer = null;
            loadedModel = null;
        }

        var gen = new ModelGenerator();

        loadedModel = gen.LoadObject(Path.Combine("models", modelName), null);

        if (loadedModel == null)
        {
            Debug.Log("Model name is incorrect!");
            errorString = string.Format("Model name: {0} is incorrect, model not found!", modelName);
            return;
        }

        loadedModel.transform.parent = transform;
        orbitCam.target = loadedModel.transform;
    }

    private void PlayAnim()
    {
        if (!SetPath()) return;
        if (loadedModel == null) return;

        if (animPlayer == null)
        {
            animPlayer = loadedModel.AddComponent<ModelAnimationPlayer>();
        }

        if (animPlayer.LoadAndSetAnimation(Path.Combine("anims", animName)) == null)
        {
            Debug.Log("Anim name is incorrect!");
            errorString = string.Format("Animation name: {0} is incorrect, animation not found!", animName);
            return;
        }

        animPlayer.isPlaying = true;
    }

    private void StopAnim()
    {
        if (loadedModel == null || animPlayer == null) return;

        animPlayer.AnimReset();
        animPlayer.isPlaying = false;
    }
}
