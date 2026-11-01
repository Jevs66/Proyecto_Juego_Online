using Fusion;
using UnityEngine;

public class BombZone : NetworkBehaviour
{
    [Header("Zone Settings")]

    [SerializeField] private int zoneIndex;

    public int ZoneIndex => zoneIndex;

    [SerializeField] private int ownerPlayerIndex;

    public int OwnerPlayerIndex => ownerPlayerIndex;

    [SerializeField] private Transform plantPoint;

    [Header("Interaction Times")]

    [SerializeField] private float plantDuration = 3f;

    [SerializeField] private float defuseDuration = 5f;

    [Networked]
    public BombZoneAction CurrentAction { get; private set; }

    [Networked]
    public float InteractionProgress { get; private set; }

    [Networked]
    public PlayerRef InteractingPlayer { get; private set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            ResetInteraction();
        }
    }

    public void ProcessInteraction(PlayerInteraction playerInteraction, bool interactPressed)
    {
        if (!HasStateAuthority || playerInteraction == null)
        {
            return;
        }

        PlayerRef player = playerInteraction.Object.InputAuthority;

        if (!interactPressed)
        {
            CancelInteraction(player);
            return;
        }

        BombController bomb = BombController.Instance;

        if (bomb == null)
        {
            CancelInteraction(player);
            return;
        }

        BombZoneAction requestedAction = GetAvailableAction(playerInteraction, bomb);

        if (requestedAction == BombZoneAction.None)
        {
            CancelInteraction(player);
            return;
        }

        if (CurrentAction != requestedAction || InteractingPlayer != player)
        {
            CurrentAction = requestedAction;
            InteractingPlayer = player;
            InteractionProgress = 0f;
        }

        InteractionProgress += Runner.DeltaTime;

        float requiredDuration = CurrentAction == BombZoneAction.Planting ? plantDuration : defuseDuration;

        if (InteractionProgress < requiredDuration)
        {
            return;
        }

        if (CurrentAction == BombZoneAction.Planting)
        {
            bomb.PlantBomb(zoneIndex, plantPoint, player);
        }
        else if (CurrentAction == BombZoneAction.Defusing)
        {
            bomb.DefuseBomb(player);
        }

        ResetInteraction();
    }

    private BombZoneAction GetAvailableAction(PlayerInteraction playerInteraction, BombController bomb)
    {
        if (playerInteraction.GetComponent<Health>().IsDead) return BombZoneAction.None;

        bool isZoneOwner = playerInteraction.TeamIndex == ownerPlayerIndex % 2 + 1;

        if (bomb.CurrentState == BombState.Carried && bomb.Carrier == playerInteraction.Object.InputAuthority && !isZoneOwner)
        {
            return BombZoneAction.Planting;
        }

        if (bomb.CurrentState == BombState.Planted && bomb.PlantedZoneIndex == zoneIndex && isZoneOwner)
        {
            return BombZoneAction.Defusing;
        }

        return BombZoneAction.None;
    }

    public void CancelInteraction(PlayerRef player)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (InteractingPlayer != player)
        {
            return;
        }

        ResetInteraction();
    }

    public void ResetInteraction()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        CurrentAction = BombZoneAction.None;
        InteractionProgress = 0f;
        InteractingPlayer = PlayerRef.None;
    }
}
