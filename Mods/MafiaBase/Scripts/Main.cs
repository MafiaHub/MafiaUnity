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

// move to MafiaGameMode.cs
class MafiaGameMode : IGameMode
{
	// on gm registration
	void IGameMode.Register()
	{
		
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
