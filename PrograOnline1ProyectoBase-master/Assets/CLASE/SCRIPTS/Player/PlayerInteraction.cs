using Fusion;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [Networked]
    public int PlayerIndex { get; private set; }

    private BombZone currentZone;

    public BombZone CurrentZone => currentZone;

    public void InitializePlayerIndex(int playerIndex)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        PlayerIndex = playerIndex;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (currentZone == null)
        {
            return;
        }

        if (MatchManager.Instance != null && MatchManager.Instance.MatchFinished)
        {
            currentZone.CancelInteraction(Object.InputAuthority);
            return;
        }

        if (!GetInput(out NetworkInfoData input))
        {
            return;
        }

        currentZone.ProcessInteraction(this, input.interactPressed);
    }

    private void OnTriggerEnter(Collider other)
    {
        BombZone bombZone = other.GetComponent<BombZone>();

        if (bombZone == null)
        {
            bombZone = other.GetComponentInParent<BombZone>();
        }

        if (bombZone != null)
        {
            currentZone = bombZone;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BombZone bombZone = other.GetComponent<BombZone>();

        if (bombZone == null)
        {
            bombZone = other.GetComponentInParent<BombZone>();
        }

        if (bombZone == null || currentZone != bombZone)
        {
            return;
        }

        currentZone.CancelInteraction(Object.InputAuthority);
        currentZone = null;
    }
}
