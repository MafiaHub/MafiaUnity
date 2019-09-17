using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MafiaUnity;

public class CustomButton
{
    private bool reset;
    private bool firstButtonPressed;
    private string buttonName;
    private float timeOfFirstButton;

    public CustomButton(string name)
    {
        buttonName = name;
    }

    public bool Button()
    {
        return Input.GetButton(buttonName);
    }

    public bool IsDoublePressed()
    {
        bool returnVal = false;
        //TODO(DavoSK): replace with get button with simillar behaviour
        if(Input.GetKeyDown(buttonName) && firstButtonPressed)
        {
            if(Time.time - timeOfFirstButton < 1f)
            {
                returnVal = true;
            }
            reset = true;
         }

        if(Input.GetKeyDown(buttonName) && !firstButtonPressed)
        {
            firstButtonPressed = true;
            timeOfFirstButton = Time.time;
        }

        if(reset)
        {
            firstButtonPressed = false;
            reset = false;
        }

        return returnVal;
    }
}
