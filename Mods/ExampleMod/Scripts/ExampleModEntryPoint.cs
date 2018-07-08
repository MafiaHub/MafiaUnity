using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using MafiaUnity;

class ScriptMain : IModScript
{
    void IModScript.Start(Mod mod)
    {
        Debug.Log("ExampleMod was initialized! Version: " + mod.version);

        var go = new GameObject("ExampleModObject");
        go.AddComponent<ExampleMonoBehaviourScript>();
        go.AddComponent<MeshFilter>();
    }
}