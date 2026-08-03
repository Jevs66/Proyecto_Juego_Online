using Fusion;
using System.Collections;
using UnityEngine;

public class NetworkProjectile : NetworkBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float projectileForce;
    [SerializeField] private float projectileLifetime;

    [Header("Impact Decal")]
    [SerializeField] private GameObject impactDecalPrefab;
    [SerializeField] private float decalDuration;
    [SerializeField] private float decalOffset;

    private Rigidbody projectileRb;
    private bool hasImpacted;

    private void Awake()
    {
        projectileRb = GetComponent<Rigidbody>();
    }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        projectileRb.AddForce(transform.forward * projectileForce, ForceMode.Impulse);

        StartCoroutine(DespawnAfterTime());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!Object.HasStateAuthority || hasImpacted)
        {
            return;
        }

        hasImpacted = true;

        ContactPoint contact = collision.GetContact(0);

        Vector3 impactPosition = contact.point + contact.normal * decalOffset;

        StartCoroutine(ImpactCoroutine(impactPosition, contact.normal));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_CreateImpactDecal(Vector3 impactPosition, Vector3 impactNormal)
    {
        if (impactDecalPrefab == null)
        {
            Debug.LogWarning("No se asignó el prefab del decal de impacto.");

            return;
        }

        Quaternion decalRotation = Quaternion.LookRotation(impactNormal);

        GameObject decal = Instantiate(impactDecalPrefab, impactPosition + impactNormal * 0.01f, decalRotation);

        Destroy(decal, decalDuration);
    }

    private IEnumerator DespawnAfterTime()
    {
        yield return new WaitForSeconds(projectileLifetime);

        Runner.Despawn(Object);
    }

    private IEnumerator ImpactCoroutine(Vector3 impactPosition, Vector3 impactNormal)
    {
        RPC_CreateImpactDecal(impactPosition, impactNormal);

        projectileRb.linearVelocity = Vector3.zero;
        projectileRb.angularVelocity = Vector3.zero;
        projectileRb.isKinematic = true;

        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(0.1f);

        if (Object != null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }
}
