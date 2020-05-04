using MafiaUnity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ModelViewer : MonoBehaviour
{
    public GUIContent panelBg;
    private string gamePath = "D:/Games/Mafia GOG clean";
    private string modelName = "Tommy.4ds";
    private string animName = "!!!Skakani.5ds";
    private string errorString = "";
    private bool repeatAnim = true;
    private GameObject loadedModel = null;
    private ModelAnimationPlayer animPlayer = null;
    private MouseOrbitImproved orbitCam = null;

    private List<string> models = new List<string>();
    private List<string> anims = new List<string>();

    private Vector2 scrollViewModels = Vector2.zero;
    private Vector2 scrollViewAnims = Vector2.zero;

    private bool modelListShown = false;
    private bool animListShown = false;

    private int modelIndex = 0;
    private int animIndex = 0;

    private void PopulateLists()
    {
        var cvars = GameAPI.instance.cvarManager;

        // Load model list first
        {
            models = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(cvars.Get("gamePath", ""), "models"));

            foreach (var f in dir.GetFiles("*.4ds"))
            {
                models.Add(f.Name);
            }
        }

        // Load anim list afterwards
        {
            anims = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(cvars.Get("gamePath", ""), "anims"));

            foreach (var f in dir.GetFiles("*.5ds"))
            {
                anims.Add(f.Name);
            }
        }
    }
    
    private void Start()
    {
        var cvars = GameAPI.instance.cvarManager;
        if (cvars.Contains("gamePath"))
        {
            var path = cvars.Get("gamePath", "C:/Games/Mafia");
            Debug.Log("Game path was detected: " + path);

            gamePath = path;
            GameAPI.instance.SetGamePath(gamePath);
            PopulateLists();
        }

        orbitCam = Camera.main.GetComponent<MouseOrbitImproved>();

        SetPath();
    }

    private void UpdateModelAndAnimFromIndices()
    {
        modelName = models[modelIndex];
        animName = anims[animIndex];

        LoadModel();
        PlayAnim();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            modelIndex--;

            if (modelIndex < 0)
                modelIndex = models.Count - 1;

            UpdateModelAndAnimFromIndices();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            modelIndex++;

            if (modelIndex == models.Count)
                modelIndex = 0;

            UpdateModelAndAnimFromIndices();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            animIndex--;

            if (animIndex < 0)
                animIndex = anims.Count - 1;

            UpdateModelAndAnimFromIndices();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            animIndex--;

            if (animIndex == anims.Count)
                animIndex = 0;

            UpdateModelAndAnimFromIndices();
        }

    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Model Viewer by ZaKlaus");
        GUILayout.Label("Hold Left Control to orbit the camera, use your mouse wheel to zoom.");
        GUILayout.Label("Use left and right arrow keys to switch models.");
        GUILayout.Label("Use up and down arrow keys to switch anims.");
        GUILayout.Label("Game path");
        gamePath = GUILayout.TextField(gamePath);

        if (GUILayout.Button("Set Path"))
        {
            SetPath();
        }

        GUILayout.Label("Model name (.4ds)");
        modelName = GUILayout.TextField(modelName);

        if (GUILayout.Button("Open model list"))
        {
            modelListShown = !modelListShown;
            animListShown = false;
        }

        if (modelListShown)
        {
            scrollViewModels = GUILayout.BeginScrollView(scrollViewModels);

            for (int i = 0; i < models.Count; i++)
            {
                if (GUILayout.Button(models[i]))
                {
                    modelName = models[i];
                    modelIndex = i;
                    LoadModel();
                }
            }

            GUILayout.EndScrollView();
        }

        GUILayout.Label("Animation name (.5ds)");
        animName = GUILayout.TextField(animName);

        if (GUILayout.Button("Open anim list"))
        {
            animListShown = !animListShown;
            modelListShown = false;
        }

        if (animListShown)
        {
            scrollViewAnims = GUILayout.BeginScrollView(scrollViewAnims);

            for (int i = 0; i < anims.Count; i++)
            {
                if (GUILayout.Button(anims[i]))
                {
                    animName = anims[i];
                    animIndex = i;
                    PlayAnim();
                }
            }

            GUILayout.EndScrollView();
        }

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

        GUILayout.EndVertical();
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
        PopulateLists();
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
