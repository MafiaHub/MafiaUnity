using UnityEngine;
using MafiaUnity;

[RequireComponent(typeof(Human))]
public class Health : MonoBehaviour
{

#region Enums
    public enum BodyPart
    {
        Head,
        Torso,
        Arm,
        Leg
    }
#endregion

#region Public Fields
    public float MaxHealth = 100.0f;
    public float CurrentHealth;

    public float PAIN_DEBOUNCE_DURATION = 3.0f;
#endregion

#region Properties
    public bool IsDead { get { return CurrentHealth <= 0.0f; } }
    public bool InPain
    {
        get {
            return (Time.time - painDebounceTime) <= PAIN_DEBOUNCE_DURATION;
        }
    }
    public BodyPart PainPart
    {
        get;
        private set;
    }
    public Human Killer
    {
        get;
        private set;
    }
#endregion

#region Private Fields
    private Human self;
    private float painDebounceTime = 0.0f; // time since last pain caused
#endregion

#region Unity
    void Awake()
    {
        self = GetComponent<Human>();
    }

    void Start()
    {
        Revive();
    }
#endregion

#region Public Methods
    public bool TakeDamage(Human attacker, float damage, BodyPart part)
    {
        damage *= GetDamageModifierBasedOnBodyPart(part);

        CurrentHealth -= damage;

        if (IsDead)
        {
            Killer = attacker;
        }
        else
        {
            CausePain(part);
        }

        return InPain;
    }

    public void Revive()
    {
        CurrentHealth = MaxHealth;
        painDebounceTime = 0.0f;
    }
#endregion

#region Private Methods
    private float GetDamageModifierBasedOnBodyPart(BodyPart part)
    {
        switch (part)
        {
            case BodyPart.Head:
                return 10.0f;
            case BodyPart.Torso:
                return 1.0f;
            case BodyPart.Arm:
                return 0.25f;
            case BodyPart.Leg:
                return 0.5f;
        }

        return 1.0f;
    }

    private void CausePain(BodyPart part)
    {
        if (InPain)
        {
            return;
        }

        float r = Random.Range(0.0f, 1.0f);

        if ((r*self.Endurance) < 0.5f)
        {
            PainPart = part;
            painDebounceTime = Time.time;
        }
    }
#endregion

}
