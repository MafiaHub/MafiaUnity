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

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        
    }
}