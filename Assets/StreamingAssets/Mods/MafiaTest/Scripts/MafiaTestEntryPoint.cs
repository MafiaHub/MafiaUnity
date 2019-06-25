using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MafiaUnity;

class ScriptMain : IModScript
{
    void IModScript.Start(Mod mod)
    {
		var gm = new MafiaTestGameMode();

		GameModeManager.instance.RegisterGameMode("MafiaTestGameModeInternal", gm);
		GameModeManager.instance.SwitchGameMode("MafiaTestGameModeInternal");
	}
}

class MafiaTestGameMode : IGameMode
{
	// on gm registration
	void IGameMode.Register()
	{
		
	}

	// on game mode switch -- being primary
	void IGameMode.Start()
	{
        var go = ObjectFactory.CreatePlayer("models/Tommy.4ds");
        go.transform.position = new Vector3(40.39561f, 20.25f, -1.018f);
/* 
		var sun = new GameObject("Sun");
		sun.transform.rotation = Quaternion.Euler(50, -30, 0);

		var sunLight = sun.AddComponent<Light>();
		sunLight.color = new Color(1f, 0.9030898f, 0.7028302f);
		sunLight.type = LightType.Directional;
        sunLight.shadows = LightShadows.Soft;
 */
		GameAPI.instance.missionManager.LoadMission("tutorial");
	}

	// on game mode switch -- leaving primary
	void IGameMode.End()
	{

	}
}
