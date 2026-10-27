using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static PhotonManager Instance;

    private NetworkRunner runner;

    [SerializeField] private UnityEvent onPlayerJoined;

    [SerializeField] private NetworkPrefabRef playerPrefab;

    [SerializeField] private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();

    [Header("Player Spawn Points")]
    [SerializeField] private Transform[] playerSpawnPoints;

    public static event Action onSessionListUpdated;
    public static List<SessionInfo> sessionList = new List<SessionInfo>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        runner = GetComponent<NetworkRunner>();
        runner.ProvideInput = true;

    }

    public async Task ConnectToServerLobby()
    {
        await runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    private void Start()
    {
        runner.AddCallbacks(this);
    }

    private async Task StartCustomMatch(GameMode mode)
    {
       
        SceneRef gameScene = SceneRef.FromIndex(0);

        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();

        if (gameScene.IsValid)
        {
            sceneInfo.AddSceneRef(gameScene, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogError("Escena no valida");
        }

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "#0001",
            Scene = sceneInfo,
            PlayerCount = 2,
            SessionProperties = new Dictionary<string, SessionProperty>
            {
                {"MapName","Abyss"},
                {"GameMode", "Casual"}
            }
        });
    }
    private async Task StartCustomMatch(GameMode mode, string serverName)
    {
       
        SceneRef gameScene = SceneRef.FromIndex(0);

        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();

        if (gameScene.IsValid)
        {
            sceneInfo.AddSceneRef(gameScene, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogError("Escena no valida");
        }

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = serverName,
            Scene = sceneInfo,
            PlayerCount = 2
        });
    }
    private async Task StartQuickMatch(GameMode mode)
    {
       
        SceneRef gameScene = SceneRef.FromIndex(0);

        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();

        if (gameScene.IsValid)
        {
            sceneInfo.AddSceneRef(gameScene, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogError("Escena no valida");
        }

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "#0001",
            Scene = sceneInfo,
            CustomLobbyName = "Lobby de pruebas"
        });
    }

    public void StartGameAsHost()
    {
        Task startGame = StartCustomMatch(GameMode.Host);
    }

    public void StartGameAsClient()
    {
        Task startGame = StartCustomMatch(GameMode.Client);
    }

    #region Metodos de Photon

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
        {
            onPlayerJoined?.Invoke();
            return;
        }

        int spawnIndex = players.Count;

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (playerSpawnPoints != null && playerSpawnPoints.Length > 0)
        {
            spawnIndex %= playerSpawnPoints.Length;

            Transform selectedSpawn = playerSpawnPoints[spawnIndex];

            if (selectedSpawn != null)
            {
                spawnPosition = selectedSpawn.position;

                spawnRotation = selectedSpawn.rotation;
            }
        }

        NetworkObject playerObject = runner.Spawn(playerPrefab, spawnPosition, spawnRotation, player);

        PlayerInteraction playerInteraction = playerObject.GetComponent<PlayerInteraction>();

        if (playerInteraction != null)
        {
            playerInteraction.InitializePlayerIndex(spawnIndex);
        }

        runner.SetPlayerObject(player, playerObject);

        players.Add(player, playerObject);

        onPlayerJoined?.Invoke();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        PhotonManager.sessionList = sessionList;
        onSessionListUpdated?.Invoke();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInfoData data = new NetworkInfoData()
        {
            movementAxis = InputManager.Instance.GetMoveInput(),
            mouseDelta = InputManager.Instance.GetMouseDelta(),

            lookRotationY = InputManager.Instance.GetLookRotationY(),

            runInputPressed = InputManager.Instance.WasRunInputPressed(),
            moveInputPressed = InputManager.Instance.IsMoveInputPressed(),
            movingBackwards = InputManager.Instance.IsMovingBackwards(),
            movingOnXAxis = InputManager.Instance.IsMovingOnXAxis(),
            isShooting = InputManager.Instance.IsShooting(),

             interactPressed = InputManager.Instance.IsInteracting()
        };

        input.Set(data);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }   

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }   

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
        {
            return;
        }

        if (!players.TryGetValue(player, out NetworkObject playerObject))
        {
            return;
        }

        if (playerObject != null && playerObject.IsValid)
        {
            runner.Despawn(playerObject);
        }

        players.Remove(player);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }    

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public bool TryGetPlayerSpawnPoint(int playerIndex, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        if (playerSpawnPoints == null || playerSpawnPoints.Length == 0)
        {
            return false;
        }

        int spawnIndex = playerIndex % playerSpawnPoints.Length;

        Transform spawnPoint = playerSpawnPoints[spawnIndex];

        if (spawnPoint == null)
        {
            return false;
        }

        position = spawnPoint.position;
        rotation = spawnPoint.rotation;

        return true;
    }
    #endregion



    #region Basura
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    } 
    #endregion
}
