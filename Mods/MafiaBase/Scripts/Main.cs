using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MafiaUnity;
using CommandTerminal;

class ScriptMain : IModScript
{
    void IModScript.Start(Mod mod)
    {
        Debug.Log("Initializing MafiaBase...");
		
		var gm = new MafiaGameMode();

		GameModeManager.instance.RegisterGameMode("_MafiaBaseGameMode", gm);
		GameModeManager.instance.SwitchGameMode("_MafiaBaseGameMode");
	}
}

class MafiaGameMode : IGameMode
{
	MenuHub menu;
	
	// on gm registration
	void IGameMode.Register()
	{
		var menuManagerObject = new GameObject("MenuManager");
        var menuManager = menuManagerObject.AddComponent<MenuManager>();
		
		menu = new MenuHub(menuManager);
	}

	// on game mode switch -- being primary
	void IGameMode.Start()
	{
		Debug.Log("MafiaGameMode WIP.");
	}

	// on game mode switch -- leaving primary
	void IGameMode.End()
	{
		Debug.Log("MafiaGameMode is shutting down... Another GM incoming!");
	}
}

class PauseMenuState : MenuState
{
    MenuHub hub;

    public PauseMenuState(MenuHub menuHub)
    {
        hub = menuHub;
    }

    public override void OnStateEnter()
	{
        MenuHelper.ToggleMouseCursor(true);
        GameManager.instance.isPaused = true;
	}

	public override void OnStateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			hub.SwitchToPlay();
		}
	}
}

class MenuHub
{
	public PauseMenuState pauseMenu;
	public PlayMenuState playMenu;
	MenuManager manager;

	public MenuHub(MenuManager menuManager)
	{
		manager = menuManager;

		pauseMenu = new PauseMenuState(this);
		playMenu = new PlayMenuState(this);

		// TODO: switch to main menu state
		SwitchToPlay();
	}

	public void SwitchToPauseMenu()
	{
		manager.SwitchMenuState(pauseMenu);
	}

	public void SwitchToPlay()
	{
		manager.SwitchMenuState(playMenu);
	}
}

// Handles the connection between pause menu, inventory screen or any other player controller unrelated actions.
class PlayMenuState : MenuState
{
	MenuHub hub;

	Texture2D hud;
	Texture2D hud2;
	
	Texture2D hpTommy;

	Texture2D ab;
	Texture2D radar;

	public PlayMenuState(MenuHub menuHub)
	{
		hub = menuHub;

		hud = TGALoader.LoadTGA(GameManager.instance.fileSystem.GetStreamFromPath("maps/1int.tga"), true);
        hud2 = TGALoader.LoadTGA(GameManager.instance.fileSystem.GetStreamFromPath("maps/2intmph.tga"), true);
        
		hpTommy = hud.CropTexture(new Rect(0, 73, 50, 22));
		ab = hud2.CropTexture(new Rect(0, 112, 35, 36));
        radar = hud2.CropTexture(new Rect(0, 147, 109, 109));
	}

	public override void OnStateEnter()
	{
        MenuHelper.ToggleMouseCursor(false);
		GameManager.instance.isPaused = false;
	}
	public override void OnStateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			hub.SwitchToPauseMenu();
		}
	}

    public override void OnStateGUI()
    { 
		if (hud == null)
			return;

        GUI.DrawTexture(new Rect(20, Screen.height - 20 - hpTommy.height, hpTommy.width, hpTommy.height), hpTommy);
        GUI.DrawTexture(new Rect(20, 20, radar.width, radar.height), radar);
        GUI.DrawTexture(new Rect(20, Screen.height - 20 - hpTommy.height - ab.height - 5, ab.width, ab.height), ab);
    }
}

class MenuHelper
{
	static public void ToggleMouseCursor(bool state)
    {
        Cursor.lockState = (state) ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }
}