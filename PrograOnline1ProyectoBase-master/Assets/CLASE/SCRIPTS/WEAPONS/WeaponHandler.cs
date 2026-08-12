using Fusion;
using System;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Weapon currentWeapon;

    public Weapon CurrentWeapon => currentWeapon;

    private Action Shoot;

    private Health health;

    private void Start()
    {
        if (currentWeapon is HandGun handGun)
        {
            Shoot = handGun.Shoot;
        }

        health = GetComponent<Health>();
    }

    public override void FixedUpdateNetwork()
    {
        if (health != null && health.IsDead)
        {
            return;
        }

        if (MatchManager.Instance != null && MatchManager.Instance.MatchFinished)
        {
            return;
        }

        if (HasInputAuthority && GetInput(out NetworkInfoData input))
        {
            if (input.isShooting)
            {
                Shoot?.Invoke();
            }
        }
    }

    public void ResetWeapons()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        foreach (Weapon weapon in weapons)
        {
            if (weapon is HandGun handGun)
            {
                handGun.ResetWeapon();
            }
        }
    }
}
