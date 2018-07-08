using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MafiaUnity;

public class SetupGUI : MonoBehaviour {

    public GameObject pathSelection;
    public GameObject modManager;
    public GameObject mainMenu;

    public void StartGame()
    {
        var modManager = GameManager.instance.modManager;
        var mods = GetComponent<ModManagerGUI>();

        foreach (var mod in mods.modEntries)
        {
            if (mod.isActive != 0)
            {
                modManager.LoadMod(mod.modName);
            }
        }

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
        }
        else
            PathSelectionMenu();

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
