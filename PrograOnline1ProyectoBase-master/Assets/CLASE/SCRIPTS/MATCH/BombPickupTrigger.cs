using UnityEngine;

public class BombPickupTrigger : MonoBehaviour
{
    [SerializeField] private BombController bombController;

    private void Awake()
    {
        if (bombController == null)
            bombController = GetComponentInParent<BombController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        bombController.TryPickUp(other);
    }
}
