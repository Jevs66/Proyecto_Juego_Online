using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandGun : Weapon
{
    [Header("HandGun Settings")]
    [SerializeField] private float shootRate;

    private float nextShootTime;

    private Transform playerRoot;

    private void Start()
    {
        if (actualAmmo <= 0)
        {
            actualAmmo = magazineSize;
        }

        MovementController movementController = GetComponentInParent<MovementController>();

        if (movementController != null)
        {
            playerRoot = movementController.transform;
        }
    }

    private void Update()
    {
        if (Object != null && !HasInputAuthority)
        {
            return;
        }

        if (MatchManager.Instance != null && MatchManager.Instance.MatchFinished)
        {
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                shootType = ShootType.Raycast;
                Debug.Log("Tipo de disparo cambiado a: Raycast");
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                shootType = ShootType.Fisico;
                Debug.Log("Tipo de disparo cambiado a: Fisico");
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                Reload();
            }
        }

    }

    public void Shoot()
    {
        if (MatchManager.Instance != null && MatchManager.Instance.MatchFinished)
        {
            return;
        }

        if (Time.time < nextShootTime)
        {
            return;
        }

        if (actualAmmo <= 0)
        {
            Debug.Log("Sin munición. Presiona R para recargar.");
            return;
        }

        nextShootTime = Time.time + shootRate;

        if (shootType == ShootType.Raycast)
        {
            RaycastShoot();
        }
        else if (shootType == ShootType.Fisico)
        {
            PhysicsShoot();
        }

        actualAmmo--;
    }

    public override void RaycastShoot()
    {
        Ray ray = GetShootRay();

        RPC_RaycastShoot(ray.origin, ray.direction);
    }

    public override void PhysicsShoot()
    {
        Transform origin = GetShootOrigin();

        Vector3 spawnPosition = origin.position + origin.forward * 0.2f;

        RPC_PhysicsShoot(spawnPosition, origin.rotation, damage);
    }

    public override void Reload()
    {
        int missingAmmo = magazineSize - actualAmmo;

        if (missingAmmo <= 0 || maxAmmoCapacity <= 0)
        {
            return;
        }

        int ammoToReload = Mathf.Min(missingAmmo, maxAmmoCapacity);
        actualAmmo += ammoToReload;
        maxAmmoCapacity -= ammoToReload;

        Debug.Log("Recargando. Munición actual: " + actualAmmo + " / Reserva: " + maxAmmoCapacity);
    }

    private Transform GetShootOrigin()
    {
        if (shootPoint != null)
        {
            return shootPoint;
        }

        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        return transform;
    }

    private Ray GetShootRay()
    {
        Transform origin = GetShootOrigin();
        return new Ray(origin.position, origin.forward);
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RaycastShoot(Vector3 rayOrigin, Vector3 rayDirection, RpcInfo info = default)
    {
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection, raycastDistance, shootLayer, QueryTriggerInteraction.Ignore);

        Array.Sort(hits, (firstHit, secondHit) => firstHit.distance.CompareTo(secondHit.distance));

        foreach (RaycastHit hit in hits)
        {
            if (playerRoot != null &&
                hit.collider.transform.root == playerRoot.root)
            {
                continue;
            }

            Debug.Log("Objeto disparado con Raycast: " + hit.collider.gameObject.name);

            Health targetHealth = hit.collider.GetComponentInParent<Health>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage, info.Source);
            }

            return;
        }

        Debug.Log("Disparo con Raycast: no golpeó ningún objeto.");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PhysicsShoot(Vector3 shootPos, Quaternion shootRot, int projectileDamage, RpcInfo info = default)
    {
        if (!projectilePrefab.IsValid)
        {
            Debug.LogError("Projectile Prefab no está asignado.");

            return;
        }

        Runner.Spawn(projectilePrefab, shootPos, shootRot, info.Source,
            (runner, spawnedObject) =>
            {
                NetworkProjectile projectile = spawnedObject.GetComponent<NetworkProjectile>();

                if (projectile != null)
                {
                    projectile.Initialize(projectileDamage, info.Source);
                }
            }
        );
    }

}
