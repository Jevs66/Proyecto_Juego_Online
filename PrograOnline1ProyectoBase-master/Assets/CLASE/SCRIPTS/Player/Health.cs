using Fusion;
using UnityEngine;

public class Health : NetworkBehaviour
{    
    [Networked] public int _actualHealth { get; private set; }

    [SerializeField]
    private int maxHealth;

    public int ActualHealth 
    { 
        get => _actualHealth;
        private set 
        { 
            _actualHealth = Mathf.Clamp(value, 0, maxHealth);
        }
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
            _actualHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {

    }

}
