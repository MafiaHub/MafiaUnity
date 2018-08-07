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
		GameManager.instance.missionManager.LoadMission("tutorial");

		var player = new GameObject("Tommy Test");
		player.AddComponent<TestPlayerController>();
	}

	// on game mode switch -- leaving primary
	void IGameMode.End()
	{

	}
}

class TestPlayerController : MonoBehaviour
{
	private void Start()
	{
		var go = new GameObject("playerController");
        var tommy = GameManager.instance.modelGenerator.LoadObject("models/Tommy.4ds");
        var player = tommy.AddComponent<ModelAnimationPlayer>();
        tommy.transform.parent = go.transform;
		
		var playerController = go.AddComponent<PlayerController>();
		playerController.playerCamera = GameObject.Find("Main Camera");
		playerController.playerPawn = tommy;
		
		var rigidBody = go.AddComponent<Rigidbody>();
		rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
		var collider = go.AddComponent<CapsuleCollider>();
		collider.center = new Vector3(0, 1f, 0);
		collider.height = 2f;
		go.transform.position = new Vector3(40.39561f, 20.25f, -1.018f);
	}
}