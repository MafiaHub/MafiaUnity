using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
	public class Door : IUsable
    {
        public MafiaFormats.Scene2BINLoader.DoorProp door;
		public int openDirection = 0;
		Quaternion startRotation, openRotation, openInvRotation, closeRotation, currentRotation;
		float delta = 0f;
		bool isMoving = false;

		AudioClip openSound, closeSound, lockedSound;

		AudioSource audioSource;

		private void Start()
		{
			audioSource = gameObject.AddComponent<AudioSource>();

			// TODO Use WAVLoader to load audio clips

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
		}

		public override void Use(GameObject user)
		{
			if (door.locked > 0)
			{
                audioSource.clip = lockedSound;
                audioSource.Play();
				return;
			}
			
			if (isMoving)
				return;
			
			isMoving = true;
			
			if (door.open > 0)
			{
				audioSource.clip = closeSound;
			}
			else
			{
                audioSource.clip = openSound;

                var vec = (user.transform.position - transform.position).normalized;
                openDirection = (Vector3.Dot(vec, transform.forward) > 0f) ? 0 : 1;
			}

			audioSource.Play();
		}
    }
}
