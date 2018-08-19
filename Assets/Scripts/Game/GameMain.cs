using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MafiaUnity;
using CommandTerminal;

class GameMain : MonoBehaviour
{
    void Start()
    {
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

        HUDManager.instance.LoadAtlas("1int", true);
		HUDManager.instance.LoadAtlas("2intmph", true);

		HUDManager.instance.LoadSprite("hpTommy", "1int", new Rect(0, 73, 50, 22));
        HUDManager.instance.LoadSprite("abButton", "2intmph", new Rect(0, 112, 35, 36));
        HUDManager.instance.LoadSprite("radar", "2intmph", new Rect(0, 147, 109, 109));

		HUDManager.instance.scale = 1.5f;
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
		var scaling = HUDManager.instance.scale;

		HUDManager.instance.DrawSprite("hpTommy", HUDAnchorMode.Bottom, new Vector2(20, 20));
		HUDManager.instance.DrawNumber(100, HUDAnchorMode.Bottom, new Vector2(25, 22));

        HUDManager.instance.DrawSprite("radar", HUDAnchorMode.None, new Vector2(20, 20));
        HUDManager.instance.DrawSprite("abButton", HUDAnchorMode.Bottom, new Vector2(20, HUDManager.instance.GetSprite("hpTommy").height*scaling + 20));
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

	public override void OnStateGUI()
	{
		GUILayout.Label("Game is paused....");

		if (GUILayout.Button("Return to Startup scene."))
		{
			// TODO
			GameManager.ResetGameManager();
			Application.LoadLevel("StartupScene");
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