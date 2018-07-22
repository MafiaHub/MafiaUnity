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
		
		Test();
	}
	
	private void Test()
	{
		var newGameObject = new GameObject("Test!").AddComponent<TestPlayerController>();
	}
}