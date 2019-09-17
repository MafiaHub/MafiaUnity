using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    public class FunnySpinPawnController : MonoBehaviour
    {
        PawnController characterController = null;

        public GameObject pawn = null;
        public MovementMode mode = MovementMode.Walk;
        public float turnAngle = 60f;

        private void FixedUpdate()
        {
            if (GameAPI.instance.isPaused)
                return;

            if (characterController == null)
            {
                if (pawn == null)
                    return;

                characterController = new PawnController(pawn.GetComponent<ModelAnimationPlayer>(), transform);
            }

            characterController.movementMode = mode;
            characterController.MoveForward();
            characterController.TurnByAngle(turnAngle);

            characterController.Update();
        }
    }
}
