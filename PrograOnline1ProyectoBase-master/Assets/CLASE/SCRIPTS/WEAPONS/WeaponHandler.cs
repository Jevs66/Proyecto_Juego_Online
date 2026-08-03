using Fusion;
using System;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Weapon currentWeapon;

    private Action Shoot;

    private void Start()
    {
        if (currentWeapon is HandGun handGun)
        {
            Shoot = handGun.Shoot;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority && GetInput(out NetworkInfoData input))
        {
            if (input.isShooting)
            {
                Shoot?.Invoke();
            }
        }
    }
}
