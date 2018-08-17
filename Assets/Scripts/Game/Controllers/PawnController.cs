using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MafiaUnity;

/// <summary>
/// Describes the speed or movement mode pawn uses
/// </summary>
public enum MovementMode
{
	Walk,
	Run,
	Crouch
}

/// <summary>
/// Pawn can have multiple animations per same action based on the stance.
/// </summary>
public enum AnimationStanceMode : int
{
    Empty,
    Pistol,
    Aim,
    Rifle,
    Shotgun,
    Crouch,
    CrouchPistol,
    CrouchAim
}

/// <summary>
/// Handles character behavior, motion and visuals. It is controlled by external classes.
/// </summary>
public class PawnController
{
	public MovementMode movementMode = MovementMode.Run;
    public AnimationStanceMode stanceMode = AnimationStanceMode.Empty;

	private ModelAnimationPlayer pawn;
    private Transform rootObject;
    private Vector3 movementDirection = Vector3.zero;
    private Vector3 oldMovementDirection;
    private float movementAngle;
	private int lastRotationState;
    public bool isRolling = false;
    
    private MafiaAnimationSet[] animationSets;

    public PawnController(ModelAnimationPlayer pawn, Transform rootObject)
    {
        this.pawn = pawn;
        this.rootObject = rootObject;
        
        animationSets = new MafiaAnimationSet[8];

        for (int i = 0; i < 8; i++)
            animationSets[i] = new MafiaAnimationSet(i+1, pawn);
        
        pawn.isPlaying = true;
        pawn.playbackMode = ModelAnimationPlayer.AnimationPlaybackMode.Repeat;
        pawn.SetAnimation(animationSets[(int)stanceMode].idleAnimations[0]);
    }

    /// <summary>
    /// Get movement speed based on the movement mode.
    /// </summary>
    /// <returns></returns>
    public float GetSpeed()
    {
        switch(movementMode)
        {
            case MovementMode.Walk:
            {
                return 2f;
            }

            case MovementMode.Run:
            {
                return 3.5f;
            }

            case MovementMode.Crouch:
            {
                return 1f;
            }
        }

        return 0f;
    }

	public void MoveForward()
	{
        movementDirection.z = 1f;
        SetMovementAnimation(AnimationSlots.Forward);
    }
	
	public void MoveBackward()
	{
        movementDirection.z = -1f;
        
        if (movementMode == MovementMode.Run)
            movementMode = MovementMode.Walk;

        SetMovementAnimation(AnimationSlots.Backward);
    }
	
	public void MoveLeft()
	{
        movementDirection.x = -1f;

        SetMovementAnimation(AnimationSlots.Left);

        if (movementDirection.z > 0f)
            SetMovementAnimation(AnimationSlots.ForwardLeft);
        else if(movementDirection.z < 0f)
            SetMovementAnimation(AnimationSlots.BackwardLeft);
	}

	public void MoveRight()
	{
        movementDirection.x = 1f;

        SetMovementAnimation(AnimationSlots.Right);

        if (movementDirection.z > 0f)
            SetMovementAnimation(AnimationSlots.ForwardRight);
        else if (movementDirection.z < 0f)
            SetMovementAnimation(AnimationSlots.BackwardRight);
    }

    public void RollLeft()
    {
        isRolling = true;
        movementDirection.x = -1f;
        movementMode = MovementMode.Run;

        pawn.OnAnimationFinish(()=> { 
            isRolling = false;
            movementDirection.x = 0f;
            movementDirection.z = 0f;
            movementMode = MovementMode.Run;
        });
        
        pawn.SetAnimation(animationSets[(int)stanceMode].jumpAnimations[(int)AnimationSlots.Left]);
        pawn.AnimReset();
    }

    public void RollRight()
    {
        isRolling = true;
        movementDirection.x = 1f;
        movementMode = MovementMode.Run;

        pawn.OnAnimationFinish(()=> { 
            isRolling = false;
            movementDirection.x = 0f;
            movementDirection.z = 0f;
            movementMode = MovementMode.Run;
        });

        pawn.SetAnimation(animationSets[(int)stanceMode].jumpAnimations[(int)AnimationSlots.Right]);
        pawn.AnimReset();
    }

    private void SetMovementAnimation(AnimationSlots slot)
    {
        switch (movementMode)
        {
            case MovementMode.Crouch:
            case MovementMode.Walk:
                pawn.SetAnimation(animationSets[(int)stanceMode].walkAnimations[(int)slot]);
                break;

            case MovementMode.Run:
                pawn.SetAnimation(animationSets[(int)stanceMode].runAnimations[(int)slot]);
                break;
        }
    }

    public void TurnByAngle(float angle)
	{
		if(angle != 0f && !isRolling)
            movementAngle = angle;
    }
	
	public void Jump()
	{
        // TODO
	}

    public void ToggleCrouch(bool state)
    {
        if (state)
        {
            if (movementMode == MovementMode.Crouch)
                return;

            movementMode = MovementMode.Crouch;

            if (stanceMode == AnimationStanceMode.Aim)
                stanceMode = AnimationStanceMode.CrouchAim;
            else if (stanceMode == AnimationStanceMode.Pistol)
                stanceMode = AnimationStanceMode.CrouchPistol;
            else
                stanceMode = AnimationStanceMode.Crouch;
        }
        else
        {
            if (movementMode != MovementMode.Crouch)
                return;

            movementMode = MovementMode.Walk;

            if (stanceMode == AnimationStanceMode.Crouch)
                stanceMode = AnimationStanceMode.Empty;
            else if (stanceMode == AnimationStanceMode.CrouchAim)
                stanceMode = AnimationStanceMode.Aim;
            else if (stanceMode == AnimationStanceMode.CrouchPistol)
                stanceMode = AnimationStanceMode.Pistol;
        }
	}
	
	public bool CanJump()
	{
        // TODO
        return false;
	}
	
	public bool CanMove()
	{
        // TODO && !isJumping
        return !isRolling;
	}
	
	public bool IsCrouched()
	{
        return movementMode == MovementMode.Crouch;
	}
	
	public bool IsMoving()
	{
        return movementDirection.magnitude > 0f;
	}

    private void UpdateRolling()
    {
        float movementSpeed = 4f * Time.deltaTime;
        rootObject.transform.Translate(movementDirection.x * movementSpeed, 0f, 0f);
    }

    public void Update()
    {
        if(isRolling)
        {
            UpdateRolling();
            return;
        }

        if (oldMovementDirection != movementDirection && IsMoving())
           pawn.AnimReset();

        if (IsMoving())
        {
            float movementSpeed = GetSpeed() * Time.deltaTime;
            oldMovementDirection = movementDirection;
            movementDirection *= movementSpeed;

            if (movementDirection.x != 0f)
            {
                rootObject.transform.Translate(movementDirection.x, 0f, 0f);
            }

            if (movementDirection.z != 0f)
            {
                rootObject.transform.Translate(0f, 0f, movementDirection.z);
            }

            movementDirection = Vector3.zero;
        }
        else
        {
            if (movementAngle > 0f)
            {
				pawn.SetAnimation(animationSets[(int)stanceMode].turnAnimations[(int)AnimationSlots.Left]);
				
				if(lastRotationState == (int)AnimationSlots.Right || lastRotationState == -1) {
					lastRotationState = (int)AnimationSlots.Left;
					pawn.AnimReset();
				}
			}
            else if(movementAngle < 0f)
            {
				pawn.SetAnimation(animationSets[(int)stanceMode].turnAnimations[(int)AnimationSlots.Right]);
				
				if(lastRotationState == (int)AnimationSlots.Left || lastRotationState == -1) {
					lastRotationState = (int)AnimationSlots.Right;
					pawn.AnimReset();
				}
            }
			else if(movementAngle == 0f)
			{
				if(lastRotationState == (int)AnimationSlots.Left || lastRotationState == (int)AnimationSlots.Right) {
					lastRotationState = -1;
					pawn.AnimReset();
				}
				
				pawn.SetAnimation(animationSets[(int)stanceMode].idleAnimations[0]);
			}
        }

		var rotToInterpolate = Quaternion.Euler(rootObject.transform.localEulerAngles.x, 
			rootObject.transform.localEulerAngles.y + movementAngle, 
			rootObject.transform.localEulerAngles.z);
        
		rootObject.transform.localRotation = Quaternion.Slerp(rootObject.transform.localRotation, rotToInterpolate, Time.deltaTime * 10f);
			
        movementAngle = 0f;
    }

    enum AnimationSlots : int
    {
        Forward,
        ForwardLeft,
        ForwardRight,
        Backward,
        BackwardLeft,
        BackwardRight,
        Left,
        Right,
    }

    public class MafiaAnimationSet
    {
        public MafiaAnimation[] walkAnimations, runAnimations, turnAnimations, jumpAnimations;
        public MafiaAnimation[] idleAnimations;

        public MafiaAnimationSet(int slot, ModelAnimationPlayer pawn)
        {
            walkAnimations  	= new MafiaAnimation[Enum.GetNames(typeof(AnimationSlots)).Length];
            runAnimations   	= new MafiaAnimation[Enum.GetNames(typeof(AnimationSlots)).Length];
			turnAnimations   	= new MafiaAnimation[Enum.GetNames(typeof(AnimationSlots)).Length]; 
            jumpAnimations      = new MafiaAnimation[Enum.GetNames(typeof(AnimationSlots)).Length];

            walkAnimations[(int)AnimationSlots.Forward] = pawn.LoadAnimation("anims/walk" + slot + ".5ds");
            walkAnimations[(int)AnimationSlots.Backward] = pawn.LoadAnimation("anims/back" + slot + ".5ds");

            walkAnimations[(int)AnimationSlots.ForwardLeft] = pawn.LoadAnimation("anims/walkL" + slot + ".5ds");
            walkAnimations[(int)AnimationSlots.ForwardRight] = pawn.LoadAnimation("anims/walkR" + slot + ".5ds");

            walkAnimations[(int)AnimationSlots.BackwardLeft] = pawn.LoadAnimation("anims/backL" + slot + ".5ds");
            walkAnimations[(int)AnimationSlots.BackwardRight] = pawn.LoadAnimation("anims/backR" + slot + ".5ds");

            walkAnimations[(int)AnimationSlots.Left] = pawn.LoadAnimation("anims/strafL" + slot + ".5ds");
            walkAnimations[(int)AnimationSlots.Right] = pawn.LoadAnimation("anims/strafR" + slot + ".5ds");

            runAnimations[(int)AnimationSlots.Forward] = pawn.LoadAnimation("anims/run" + slot + ".5ds");
            runAnimations[(int)AnimationSlots.Backward] = pawn.LoadAnimation("anims/back" + slot + ".5ds");

            runAnimations[(int)AnimationSlots.ForwardLeft] = pawn.LoadAnimation("anims/runL" + slot + ".5ds");
            runAnimations[(int)AnimationSlots.ForwardRight] = pawn.LoadAnimation("anims/runR" + slot + ".5ds");

            runAnimations[(int)AnimationSlots.BackwardLeft] = pawn.LoadAnimation("anims/backL" + slot + ".5ds");
            runAnimations[(int)AnimationSlots.BackwardRight] = pawn.LoadAnimation("anims/backR" + slot + ".5ds");

            runAnimations[(int)AnimationSlots.Left] = pawn.LoadAnimation("anims/strafL" + slot + ".5ds");
            runAnimations[(int)AnimationSlots.Right] = pawn.LoadAnimation("anims/strafR" + slot + ".5ds");

			turnAnimations[(int)AnimationSlots.Left] = pawn.LoadAnimation("anims/left" + slot + ".5ds");
            turnAnimations[(int)AnimationSlots.Right] = pawn.LoadAnimation("anims/left" + slot + ".5ds"); // temp fix
			
			jumpAnimations[(int)AnimationSlots.Left] = pawn.LoadAnimation("anims/jumpL1.5ds");
            jumpAnimations[(int)AnimationSlots.Right] = pawn.LoadAnimation("anims/jumpR1.5ds");

            idleAnimations = new MafiaAnimation[]{ pawn.LoadAnimation("anims/breath0" + slot + "a.5ds"), 
                               pawn.LoadAnimation("anims/breath0" + slot + "b.5ds"),
                               pawn.LoadAnimation("anims/breath0" + slot + "c.5ds"), 
                               pawn.LoadAnimation("anims/breath0" + slot + "d.5ds")};
        }
    }
}