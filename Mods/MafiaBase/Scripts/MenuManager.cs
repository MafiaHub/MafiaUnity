using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MafiaUnity;

class MenuManager : MonoBehaviour
{
    MenuState state;

    public void SwitchMenuState(MenuState state)
    {
        if (this.state != null)
            this.state.OnStateLeave();

        this.state = state;

        if (this.state != null)
            this.state.OnStateEnter();
    }

    private void Update()
    {
        if (state == null)
            return;

        state.OnStateUpdate();
    }

    private void OnGUI()
    {
        if (state == null)
            return;
        
        state.OnStateGUI();
    }
}

abstract class MenuState
{
    public virtual void OnStateEnter()
    {}

    public virtual void OnStateLeave()
    {}

    public virtual void OnStateUpdate()
    {}

    public virtual void OnStateGUI()
    {}
}
