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
    private Transform neckTransform;
    private float cameraUpAndDown = 2.01f;
    private CustomButton leftButton = new CustomButton("a");
    private CustomButton rightButton = new CustomButton("d");
    Vector3 neckStandPosition, neckCrouchPosition;
    const float CROUCH_CAMERA_DOWN = 0.8f;

    public void Start()
    {
        characterController = new PawnController(playerPawn.GetComponent<ModelAnimationPlayer>(), transform);
        playerCamera.transform.position = CalculateAndUpdateCameraPosition();

        neckTransform = transform.FindDeepChild("neck");
        cameraOrbitPoint = new GameObject("cameraOrbitPoint").transform;
        cameraOrbitPoint.parent = transform;
        cameraOrbitPoint.position = neckTransform.position;
        neckCrouchPosition = neckStandPosition = cameraOrbitPoint.localPosition;
        neckCrouchPosition.y -= CROUCH_CAMERA_DOWN;
    }

    private Vector3 CalculateAndUpdateCameraPosition()
    {
        var dir = transform.forward * -1.46f;
        var pos = transform.position + dir;
        pos.y += cameraUpAndDown;

        if (characterController.IsCrouched())
        {
            pos.y -= CROUCH_CAMERA_DOWN;

            if (cameraOrbitPoint != null)
                cameraOrbitPoint.localPosition = Vector3.Lerp(cameraOrbitPoint.localPosition, neckCrouchPosition, Time.deltaTime * 10f);
        }
        else if (cameraOrbitPoint != null)
        {
            cameraOrbitPoint.localPosition = Vector3.Lerp(cameraOrbitPoint.localPosition, neckStandPosition, Time.deltaTime * 10f);
        }

        return pos;
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

        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, CalculateAndUpdateCameraPosition(), Time.deltaTime * 10f);
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

    public void FixedUpdate()
    {
        if (GameManager.instance.isPaused)
            return;
            
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