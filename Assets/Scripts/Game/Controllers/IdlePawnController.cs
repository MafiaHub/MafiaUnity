using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
	public class IdlePawnController : MonoBehaviour 
	{
		PawnController characterController = null;

		public GameObject pawn = null;

		private void Start()
		{
			characterController = new PawnController(pawn.GetComponent<ModelAnimationPlayer>(), transform);
		}

		private void FixedUpdate()
		{
            if (GameAPI.instance.isPaused)
                return;

			if (characterController == null)
				return;
				
			characterController.Update();
		}
	}
}