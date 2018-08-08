using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MafiaUnity;

public class SetupGUI : MonoBehaviour {

    public GameObject pathSelection;
    public GameObject modManager;
    public GameObject mainMenu;
    public GameObject startupLight;

    GameObject background;

    public void StartGame()
    {
        var modManager = GameManager.instance.modManager;
        var mods = GetComponent<ModManagerGUI>();

        foreach (var mod in mods.modEntries)
        {
            if (mod.status != 0)
            {
                modManager.LoadMod(mod.modName);
            }
        }

        // Revert settings back to default.
        RenderSettings.ambientLight = new Color(54, 58, 66);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

        GameObject.Destroy(startupLight);
        GameObject.Destroy(background);
        GameObject.Destroy(GameObject.Find("EventSystem"));
        GameObject.Destroy(gameObject);
    }

    public void PathSelectionMenu()
    {
        mainMenu.SetActive(false);
        pathSelection.SetActive(true);
    }

    public void ModManagerMenu()
    {
        mainMenu.SetActive(false);
        modManager.SetActive(true);
    }

    // Use this for initialization
    void Start() {
        if (PlayerPrefs.HasKey("gamePath"))
        {
            if (!GameManager.instance.SetGamePath(PlayerPrefs.GetString("gamePath")))
                PathSelectionMenu();
            else
                SetupDefaultBackground();
        }
        else
            PathSelectionMenu();

    }

    bool bgWasSetup = false;

    public void SetupDefaultBackground()
    {
        if (bgWasSetup)
            return;

        if (GameManager.instance.GetInitialized())
        {
            bgWasSetup = true;

            var scenery = GameManager.instance.modelGenerator.LoadObject("missions/00menu/scene.4ds");
            background = GameManager.instance.sceneGenerator.LoadObject("missions/00menu/scene2.bin");
            scenery.transform.SetParent(background.transform);
        }
    }
}
