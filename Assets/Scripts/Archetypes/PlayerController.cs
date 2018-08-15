using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MafiaUnity;

public class PlayerController : MonoBehaviour
{
    private PawnController characterController;
    public GameObject playerCamera;
    public GameObject playerPawn;
    private Transform cameraOrbitPoint;
    private float cameraUpAndDown = 2.01f;
    private CustomButton leftButton = new CustomButton("a");
    private CustomButton rightButton = new CustomButton("d");

    public void Start()
    {
        characterController = new PawnController(playerPawn.GetComponent<ModelAnimationPlayer>(), transform);
        playerCamera.transform.parent = transform;
        playerCamera.transform.localPosition = new Vector3(0f, cameraUpAndDown, -1.46f);

        var playerNeckTrans = transform.FindDeepChild("neck");
        var newObject = GameObject.Instantiate(playerNeckTrans.gameObject);
        newObject.transform.parent = transform;
        newObject.transform.position = playerNeckTrans.position;
        newObject.name = "cameraOrbitPoint";
        cameraOrbitPoint = newObject.transform;
    }

    private void UpdateCameraMovement()
    {
        var x = Input.GetAxis("Mouse X") * Time.deltaTime * 800f;
        var y = Input.GetAxis("Mouse Y") * Time.deltaTime * 5f;

        cameraUpAndDown -= y;

        if (cameraUpAndDown < 0.9f)
            cameraUpAndDown = 0.9f;

        if (cameraUpAndDown > 3.5f)
            cameraUpAndDown = 3.5f;

        playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, new Vector3(0f, cameraUpAndDown, -1.46f), Time.deltaTime * 10f);
        playerCamera.transform.LookAt(cameraOrbitPoint);
        characterController.TurnByAngle(x);
    }

    private int xCountPressed = 0;
    private int zCountPressed = 0;


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

    public void Update()
    {
        if (characterController == null)
            return;
            
        var x = Input.GetAxisRaw("Horizontal");
        var z = Input.GetAxisRaw("Vertical");
        var isRunning = !Input.GetButton("Run");
        var isCrouching = Input.GetButton("Crouch");
        
        if(!characterController.isRolling)
        {
            if (isCrouching)
                characterController.ToggleCrouch(true);
            else
                characterController.ToggleCrouch(false);
            
            if (isRunning && !isCrouching)
                characterController.movementMode = MovementMode.Run;
            else if (!isCrouching)
                characterController.movementMode = MovementMode.Walk;

            if(leftButton.IsDoublePressed())
                characterController.RollLeft();

            if(rightButton.IsDoublePressed())
                characterController.RollRight();

            //Check even here due to code bellow :/
            if(characterController.isRolling) return;

            if (z > 0f)
            {
                characterController.MoveForward();
            }
            else if (z < 0f)
            {
                characterController.MoveBackward();
            }

            if (x > 0f)
            {
                characterController.MoveRight();
            }
            else if (x < 0f)
            {
                characterController.MoveLeft();
            }
        }

        characterController.Update();
        
        UpdateCameraMovement();
    }
}