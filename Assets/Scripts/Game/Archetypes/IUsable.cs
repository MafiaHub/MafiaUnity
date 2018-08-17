using UnityEngine;

public abstract class IUsable : MonoBehaviour
{
	public virtual void Use(GameObject user)
	{
		
	}

    public virtual void UseDoor(GameObject user, int doorSide)
	{

	}
}
