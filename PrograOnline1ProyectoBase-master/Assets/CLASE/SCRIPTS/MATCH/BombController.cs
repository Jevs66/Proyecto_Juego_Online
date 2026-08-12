using Fusion;
using UnityEngine;

public class BombController : NetworkBehaviour
{
    public static BombController Instance { get; private set; }

    [Networked, OnChangedRender(nameof(OnBombStateChanged))]
    public BombState CurrentState { get; private set; }

    [Networked]
    public PlayerRef Carrier { get; private set; }

    [Networked]
    public PlayerRef Planter { get; private set; }

    [Networked]
    public int PlantedZoneIndex { get; private set; }

    [Networked]
    public float ExplosionTimeRemaining { get; private set; }

    [SerializeField] private float explosionDuration;

    [SerializeField] private Vector3 carriedPositionOffset;

    [SerializeField] private Vector3 carriedRotationOffset;

    [SerializeField] private Collider physicalCollider;

    [SerializeField] private Collider pickupTrigger;

    private Rigidbody bombRigidbody;
    private PlayerBombHolder currentHolder;

    private Vector3 initialPosition;

    private Quaternion initialRotation;

    private void Awake()
    {
        bombRigidbody = GetComponent<Rigidbody>();
    }

    public override void Spawned()
    {
        Instance = this;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (HasStateAuthority)
        {
            CurrentState = BombState.OnGround;
            Carrier = PlayerRef.None;

            Planter = PlayerRef.None;
            PlantedZoneIndex = -1;
            ExplosionTimeRemaining = 0f;
        }

        UpdateBombComponents();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (CurrentState == BombState.Carried)
        {
            FollowCarrier();
        }

        if (CurrentState == BombState.Planted)
        {
            ExplosionTimeRemaining -= Runner.DeltaTime;

            if (ExplosionTimeRemaining <= 0f)
            {
                ExplodeBomb();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (CurrentState != BombState.OnGround)
        {
            return;
        }

        if (MatchManager.Instance != null && MatchManager.Instance.MatchFinished)
        {
            return;
        }

        Health playerHealth = other.GetComponentInParent<Health>();

        if (playerHealth == null || playerHealth.IsDead)
        {
            return;
        }

        PlayerRef player = playerHealth.Object.InputAuthority;

        if (player == PlayerRef.None)
        {
            return;
        }

        PickUp(player);
    }

    private void FollowCarrier()
    {
        if (Carrier == PlayerRef.None)
        {
            return;
        }

        if (currentHolder == null)
        {
            currentHolder = FindHolder(Carrier);
        }

        if (currentHolder == null || currentHolder.BombHolder == null)
        {
            return;
        }

        Transform holderTransform = currentHolder.BombHolder;

        transform.position = holderTransform.TransformPoint(carriedPositionOffset);
        transform.rotation = holderTransform.rotation * Quaternion.Euler(carriedRotationOffset);
    }

    private void PickUp(PlayerRef player)
    {
        if (!HasStateAuthority || CurrentState != BombState.OnGround)
        {
            return;
        }

        PlayerBombHolder holder = FindHolder(player);

        if (holder == null || holder.BombHolder == null)
        {
            Debug.LogWarning("El jugador " + player + " no tiene un BombHolder configurado.");
            return;
        }

        Carrier = player;
        CurrentState = BombState.Carried;
        currentHolder = holder;

        UpdateBombComponents();

        Debug.Log("El jugador " + player + " recogió la bomba.");
    }

    public void DropIfCarriedBy(PlayerRef player)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (CurrentState != BombState.Carried || Carrier != player)
        {
            return;
        }

        DropBomb();
    }

    private void DropBomb()
    {
        Carrier = PlayerRef.None;
        CurrentState = BombState.OnGround;
        currentHolder = null;

        UpdateBombComponents();

        Debug.Log("La bomba cayó al suelo.");
    }

    public void PlantBomb(int deviceIndex, Transform plantPoint, PlayerRef planter)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (CurrentState != BombState.Carried || Carrier != planter)
        {
            return;
        }

        if (plantPoint == null)
        {
            Debug.LogError("El dispositivo no tiene un PlantPoint asignado.");
            return;
        }

        Planter = planter;
        Carrier = PlayerRef.None;
        PlantedZoneIndex = deviceIndex;
        ExplosionTimeRemaining = explosionDuration;
        CurrentState = BombState.Planted;
        currentHolder = null;

        transform.position = plantPoint.position;
        transform.rotation = plantPoint.rotation;

        UpdateBombComponents();

        Debug.Log("El jugador " + planter + " plantó la bomba.");
    }

    public void DefuseBomb(PlayerRef defuser)
    {
        if (!HasStateAuthority || CurrentState != BombState.Planted)
        {
            return;
        }

        CurrentState = BombState.Defused;
        ExplosionTimeRemaining = 0f;

        UpdateBombComponents();

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.EndMatch(defuser, WinConditions.BombDefused);
        }

        Debug.Log("El jugador " + defuser + " desactivó la bomba.");
    }

    private void ExplodeBomb()
    {
        if (!HasStateAuthority || CurrentState != BombState.Planted)
        {
            return;
        }

        ExplosionTimeRemaining = 0f;
        CurrentState = BombState.Exploded;

        UpdateBombComponents();

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.EndMatch(Planter, WinConditions.BombExploded);
        }

        Debug.Log("La bomba explotó.");
    }

    public void ResetBomb()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        Carrier = PlayerRef.None;
        Planter = PlayerRef.None;
        PlantedZoneIndex = -1;
        ExplosionTimeRemaining = 0f;
        CurrentState = BombState.OnGround;

        currentHolder = null;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (bombRigidbody != null)
        {
            bombRigidbody.linearVelocity = Vector3.zero;
            bombRigidbody.angularVelocity = Vector3.zero;
        }

        UpdateBombComponents();

        Debug.Log("Bomba reiniciada.");
    }

    private PlayerBombHolder FindHolder(PlayerRef player)
    {
        if (Runner == null)
        {
            return null;
        }

        NetworkObject playerObject = Runner.GetPlayerObject(player);

        if (playerObject == null)
        {
            return null;
        }

        return playerObject.GetComponent<PlayerBombHolder>();
    }

    private void UpdateBombComponents()
    {
        bool isOnGround = CurrentState == BombState.OnGround;

        if (physicalCollider != null)
        {
            physicalCollider.enabled = isOnGround;
            physicalCollider.isTrigger = false;
        }

        if (pickupTrigger != null)
        {
            pickupTrigger.enabled = isOnGround;
            pickupTrigger.isTrigger = true;
        }

        if (bombRigidbody != null)
        {
            bombRigidbody.isKinematic = !isOnGround;
            bombRigidbody.useGravity = isOnGround;

            if (!isOnGround)
            {
                bombRigidbody.linearVelocity = Vector3.zero;
                bombRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

    private void OnBombStateChanged()
    {
        UpdateBombComponents();

        if (CurrentState == BombState.Carried)
        {
            currentHolder = FindHolder(Carrier);
        }
        else
        {
            currentHolder = null;
        }
    }

    public void TryPickUp(Collider other)
    {
        if (!HasStateAuthority)
            return;

        if (CurrentState != BombState.OnGround)
            return;

        if (MatchManager.Instance != null && MatchManager.Instance.MatchFinished)
            return;

        Health playerHealth = other.GetComponentInParent<Health>();

        if (playerHealth == null || playerHealth.IsDead)
            return;

        PlayerRef player = playerHealth.Object.InputAuthority;

        if (player == PlayerRef.None)
            return;

        PickUp(player);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
