using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MafiaUnity;

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
		var go = new GameObject("DebugMode");
		go.AddComponent<DebugConsole>();

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

	public PlayMenuState(MenuHub menuHub)
	{
		hub = menuHub;
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

    
}

class MenuHelper
{
	static public void ToggleMouseCursor(bool state)
    {
        Cursor.lockState = (state) ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }
}