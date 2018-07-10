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

    private Quaternion rotToInterpolate;
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

    public void Update()
    {
        var x = Input.GetAxisRaw("Horizontal");
        var z = Input.GetAxisRaw("Vertical");
        var isRunning = !Input.GetButton("Run");
        var isCrouching = Input.GetButton("Crouch");
        
        if (isCrouching)
        {
            characterController.ToggleCrouch(true);
        }
        else
        {
            characterController.ToggleCrouch(false);
        }

        if (isRunning && !isCrouching)
            characterController.movementMode = MovementMode.Run;
        else if (!isCrouching)
            characterController.movementMode = MovementMode.Walk;

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

        characterController.Update();
        UpdateCameraMovement();
    }
}