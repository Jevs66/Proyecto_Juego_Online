using UnityEngine;
using Fusion;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance { get; private set; }

    [Networked, OnChangedRender(nameof(OnMatchStateChanged))]
    public MatchState CurrentState { get; private set; }

    [Networked]
    public PlayerRef Winner { get; private set; }

    [Networked]
    public int WinnerTeam { get; private set; }

    [Networked]
    public WinConditions CurrentWinCondition { get; private set; }

    public bool MatchFinished => CurrentState == MatchState.Finished;

    private bool restartPending;

    public override void Spawned()
    {
        Instance = this;

        if (HasStateAuthority)
        {
            CurrentState = MatchState.Playing;
            Winner = PlayerRef.None;
            WinnerTeam = 0;
            CurrentWinCondition = WinConditions.None;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (!restartPending)
        {
            return;
        }

        restartPending = false;
        RestartMatch();
    }

    public void PlayerDied(PlayerRef deadPlayer)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (CurrentState == MatchState.Finished)
        {
            return;
        }

        PlayerRef winner = FindOpponent(deadPlayer);

        if (winner == PlayerRef.None) return;

        EndMatch(winner, WinConditions.PlayerKilled);
    }

    private PlayerRef FindOpponent(PlayerRef deadPlayer)
    {
        int deadTeam = Runner.GetPlayerObject(deadPlayer).GetComponent<PlayerInteraction>().TeamIndex;
        PlayerRef winner = PlayerRef.None;

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            NetworkObject playerObject = Runner.GetPlayerObject(player);

            if (playerObject.GetComponent<Health>().IsDead) continue;

            if (playerObject.GetComponent<PlayerInteraction>().TeamIndex == deadTeam) return PlayerRef.None;

            winner = player;
        }

        return winner;
    }

    public void EndMatch(PlayerRef winner, WinConditions winCondition)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (CurrentState == MatchState.Finished)
        {
            return;
        }

        Winner = winner;
        WinnerTeam = Runner.GetPlayerObject(winner).GetComponent<PlayerInteraction>().TeamIndex;
        CurrentWinCondition = winCondition;
        CurrentState = MatchState.Finished;

        RPC_ShowMatchResult(WinnerTeam, winCondition);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowMatchResult(int winnerTeam, WinConditions winCondition)
    {
        bool localPlayerWon = Runner.GetPlayerObject(Runner.LocalPlayer).GetComponent<PlayerInteraction>().TeamIndex == winnerTeam;

        if (localPlayerWon)
        {
            Debug.Log("GANASTE. Razón: " + winCondition);
        }
        else
        {
            Debug.Log("PERDISTE. Razón: " + winCondition);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RequestRestart()
    {
        if (Runner == null)
        {
            return;
        }

        if (HasStateAuthority)
        {
            restartPending = true;
            return;
        }

        RPC_RequestRestart();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestRestart(RpcInfo info = default)
    {
        if (info.Source != PlayerRef.FromIndex(0))
        {
            Debug.LogWarning("El jugador " + info.Source + " intentó reiniciar la partida.");
            return;
        }

        restartPending = true;
    }

    private void RestartMatch()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        CurrentState = MatchState.Playing;
        Winner = PlayerRef.None;
        WinnerTeam = 0;
        CurrentWinCondition = WinConditions.None;

        ResetPlayers();
        ResetBomb();
        ResetZones();

        RPC_ResetMatchVisuals();

        Debug.Log("PARTIDA REINICIADA.");
    }

    private void ResetPlayers()
    {
        PhotonManager photonManager = FindFirstObjectByType<PhotonManager>();

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            NetworkObject playerObject = Runner.GetPlayerObject(player);

            if (playerObject == null)
            {
                continue;
            }

            PlayerInteraction playerInteraction = playerObject.GetComponent<PlayerInteraction>();
            Health health = playerObject.GetComponent<Health>();
            MovementController movementController = playerObject.GetComponent<MovementController>();
            WeaponHandler weaponHandler = playerObject.GetComponent<WeaponHandler>();

            if (health != null)
            {
                health.ResetHealth();
            }

            if (photonManager != null && playerInteraction != null && movementController != null)
            {
                if (photonManager.TryGetPlayerSpawnPoint(playerInteraction.PlayerIndex, out Vector3 spawnPosition, out Quaternion spawnRotation))
                {
                    movementController.ResetPlayerPosition(spawnPosition, spawnRotation);
                }
            }

            if (weaponHandler != null)
            {
                weaponHandler.ResetWeapons();
            }
        }
    }

    private void ResetBomb()
    {
        if (BombController.Instance == null)
        {
            return;
        }

        BombController.Instance.ResetBomb();
    }

    private void ResetZones()
    {
        BombZone[] zones = FindObjectsByType<BombZone>(FindObjectsSortMode.None);

        foreach (BombZone zone in zones)
        {
            zone.ResetInteraction();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ResetMatchVisuals()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnMatchStateChanged()
    {
        if (CurrentState != MatchState.Finished)
        {
            return;
        }

        Debug.Log("Partida terminada. Equipo ganador: " + WinnerTeam + " | Razón: " + CurrentWinCondition);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
