using UnityEngine;
using MafiaUnity;

public class Human : MonoBehaviour
{
#region Public Fields
    public float Precision = 1.0f; // determines shooting precision
    public float Aggressiveness = 0.0f; // passive by default
    public float Strength = 0.5f; // determines melee strength
    public float Endurance = 0.5f; // determines how often the human becomes unable to fight due to pain

    public static float MIN_COMBAT_TRIGGER_DISTANCE = 15.0f; // scales by aggressiveness
#endregion
}
