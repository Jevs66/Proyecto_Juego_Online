using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Weapon : NetworkBehaviour
{
    [Header("Shoot Type")]
    [SerializeField] public ShootType shootType;

    [Header("Weapon Stats")]
    public int damage;
    public int actualAmmo;
    public int magazineSize;
    public int maxAmmoCapacity;

    [Header("Shoot Settings")]
    public NetworkPrefabRef projectilePrefab;
    public Transform shootPoint;
    public LayerMask shootLayer;
    public float raycastDistance;
    public float projectileForce;

    public abstract void RaycastShoot();

    public abstract void PhysicsShoot();

    public abstract void Reload();
}
