using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using MafiaUnity;

class ScriptMain : IModScript
{
    void IModScript.Start()
    {
        Debug.Log("ExampleMod was initialized!");

        var go = new GameObject("ExampleModObject");
        go.AddComponent<ExampleMonoBehaviourScript>();
        go.AddComponent<MeshFilter>();
    }
}