using Fusion;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int ActualHealth { get; private set; }

    [Networked]
    public NetworkBool IsDead { get; private set; }

    [SerializeField]
    private int maxHealth = 100; 

    public int MaxHealth => maxHealth;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            ActualHealth = maxHealth;
            IsDead = false;
        }
    }

    public void TakeDamage(int damage, PlayerRef attacker)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (IsDead || damage <= 0)
        {
            return;
        }

        ActualHealth = Mathf.Clamp(
            ActualHealth - damage,
            0,
            maxHealth
        );

        Debug.Log(
            Object.InputAuthority + " recibio " + damage + " de danio. Vida restante: " +
            ActualHealth
        );

        if (ActualHealth <= 0)
        {
            Die(attacker);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int damage, PlayerRef attacker, RpcInfo info = default)
    {
        TakeDamage(damage, attacker);
    }

    private void Die(PlayerRef attacker)
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        ActualHealth = 0;

        Debug.Log("Jugador eliminado: " + Object.InputAuthority + " | Atacante: " + attacker);

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.PlayerDied(Object.InputAuthority);
        }
        else
        {
            Debug.LogError("No existe un MatchManager en la escena.");
        }
    }

    private void OnHealthChanged()
    {
        Debug.Log("Vida actual de " + Object.InputAuthority + ": " + ActualHealth);
    }

}
