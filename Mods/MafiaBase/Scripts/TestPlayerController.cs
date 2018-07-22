using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MafiaUnity;

class TestPlayerController : MonoBehaviour
{
    void Start()
	{
		GameManager.instance.missionManager.LoadMission("tutorial");
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

//13:30