using System;
using System.Collections;
using System.Collections.Generic;

public class GameModeManager
{
    #region Singleton
    static GameModeManager instanceObject;
    public static GameModeManager instance
    {
        get
        {
            if (instanceObject == null)
                instanceObject = new GameModeManager();
            
            return instanceObject;
        }
    }
    #endregion


    Dictionary<string, IGameMode> modes = new Dictionary<string, IGameMode>();
    IGameMode currentMode;

    public void RegisterGameMode(string name, IGameMode mode)
    {
        if (mode == null)
            return;

        if (modes.ContainsKey(name))
            return;

        mode.Register();

        modes.Add(name, mode);
    }

    public void SwitchGameMode(string name)
    {
        if (!modes.ContainsKey(name))
            return;

            
        if (currentMode != null)
        {
            currentMode.End();
        }

        currentMode = modes[name];

        currentMode.Start();
    }
}