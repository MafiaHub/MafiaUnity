using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
	public class Door : IUsable
    {
        public MafiaFormats.Scene2BINLoader.DoorProp door;
		Quaternion startRotation, openRotation, openInvRotation, closeRotation, currentRotation;
		float delta = 0f;
		bool isMoving = false;
		int openDirection = 0;

		private void Start()
		{
			startRotation = transform.localRotation;

			if (door.open > 0)
			{
                closeRotation = startRotation;
				// TODO figure these out
				if (door.open1 > 0)
                    transform.Rotate(new Vector3(0, door.moveAngle * Mathf.Rad2Deg, 0), Space.Self);
				else if (door.open2 > 0)
                    transform.Rotate(new Vector3(0, -1 * door.moveAngle * Mathf.Rad2Deg, 0), Space.Self);
				else
                    transform.Rotate(new Vector3(0, door.moveAngle * Mathf.Rad2Deg, 0), Space.Self);
				
				openRotation = transform.localRotation;
			}
			else
			{
				closeRotation = startRotation;
				openRotation = Quaternion.Euler(startRotation.eulerAngles.x, startRotation.eulerAngles.y + door.moveAngle*Mathf.Rad2Deg, startRotation.eulerAngles.z);
			}

			openInvRotation = Quaternion.Euler(openRotation.eulerAngles.x, startRotation.eulerAngles.y - door.moveAngle*Mathf.Rad2Deg, openRotation.eulerAngles.z);
            currentRotation = transform.localRotation;
		}

		private void Update()
		{
			if (isMoving)
			{
				if (door.open > 0)
				{
					transform.localRotation = Quaternion.Lerp(currentRotation, closeRotation, delta);
				}
				else
				{
                    var finalOpenRotation = (openDirection == 0) ? openRotation : openInvRotation;
                    transform.localRotation = Quaternion.Lerp(closeRotation, finalOpenRotation, delta);
				}

				delta += Time.deltaTime;

				if (delta > 1f)
				{
					delta = 0f;
					isMoving = false;
					door.open = (byte)(1 - door.open);
                    currentRotation = transform.localRotation;
				}
			}	

			// debug
			/* if (Input.GetKeyDown(KeyCode.O))
			{
				UseDoor(gameObject, 0);
			}

            if (Input.GetKeyDown(KeyCode.I))
            {
                UseDoor(gameObject, 1);
            } */
		}

		public void UseDoor(GameObject user, int doorSide)
		{
			if (door.locked > 0)
			{
				// TODO play locked sound
				return;
			}
			
			if (isMoving)
				return;
			
			isMoving = true;
			openDirection = doorSide;
			Debug.Log("Door status: " + door.open);
		}
    }
}
