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
    [SerializeField]  private Transform[] playerSpawnPoints;

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

        DontDestroyOnLoad(gameObject);

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
        MenuNavigation.Instance.ShowSessionLoading(mode == GameMode.Host ? "Creating server..." : "Joining server...");

        var gameScene = SceneRef.FromIndex(1);

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(gameScene, LoadSceneMode.Single); 

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = mode == GameMode.Client ? null : "01",
            Scene = sceneInfo,
            PlayerCount = 2,
            SceneManager = GetComponent<NetworkSceneManagerDefault>(), 
            SessionProperties = new Dictionary<string, SessionProperty>
            {
                {"MapName", "Abyss"},
                {"GameMode", "Casual"}
            }
        });
    }
    private async Task StartCustomMatch(GameMode mode, string serverName, int maxPlayers = 10)
    {
        MenuNavigation.Instance.ShowSessionLoading(mode == GameMode.Host ? "Creating server..." : "Joining server...");

        var gameScene = SceneRef.FromIndex(1);

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(gameScene, LoadSceneMode.Single);

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = serverName,
            Scene = sceneInfo,
            PlayerCount = maxPlayers, 
            SceneManager = GetComponent<NetworkSceneManagerDefault>(),
            SessionProperties = new Dictionary<string, SessionProperty>
            {
                {"MapName", "Abyss"},
                {"GameMode", "Casual"}
            }
        });
    }

    private async Task StartQuickMatch(GameMode mode)
    {
        var gameScene = SceneRef.FromIndex(1); 

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(gameScene, LoadSceneMode.Single); 

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "#0001",
            Scene = sceneInfo,
            CustomLobbyName = "Lobby de pruebas",
            SceneManager = GetComponent<NetworkSceneManagerDefault>() 
        });
    }

    public void StartGameAsHost()
    {
        Task startGame = StartCustomMatch(GameMode.Host);
    }

    public void StartGameAsHost(string serverName, int maxPlayers)
    {
        Task startGame = StartCustomMatch(GameMode.Host, serverName, maxPlayers);
    }

    public void StartGameAsClient()
    {
        Task startGame = StartCustomMatch(GameMode.Client);
    }

    public void StartGameAsClient(string serverName)
    {
        Task startGame = StartCustomMatch(GameMode.Client, serverName);
    }

    #region Metodos de Photon

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
        {
            onPlayerJoined?.Invoke();
            return;
        }

        if (playerSpawnPoints != null) SpawnPlayer(runner, player);

        onPlayerJoined?.Invoke();
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (players.ContainsKey(player) || playerSpawnPoints == null || playerSpawnPoints.Length == 0) return;

        bool[] usedSpawns = new bool[playerSpawnPoints.Length];

        foreach (NetworkObject playerObject in players.Values)
        {
            usedSpawns[playerObject.GetComponent<PlayerInteraction>().PlayerIndex] = true;
        }

        int spawnIndex = Array.IndexOf(usedSpawns, false);

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        Transform selectedSpawn = playerSpawnPoints[spawnIndex];

        if (selectedSpawn != null)
        {
            spawnPosition = selectedSpawn.position;
            spawnRotation = selectedSpawn.rotation;
        }

        NetworkObject playerObjectNew = runner.Spawn(playerPrefab, spawnPosition, spawnRotation, player); 

        PlayerInteraction playerInteraction = playerObjectNew.GetComponent<PlayerInteraction>();

        if (playerInteraction != null)
        {
            playerInteraction.InitializePlayerIndex(spawnIndex);
        }

        runner.SetPlayerObject(player, playerObjectNew); 

        players.Add(player, playerObjectNew); 
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
        if (!SceneManager.GetSceneByBuildIndex(1).isLoaded) return;

        GameObject spawnContainer = GameObject.Find("Spawns");

        if (spawnContainer == null || spawnContainer.transform.childCount == 0)
        {
            Debug.LogError("No hay SpawnPoints");
            return;
        }

        playerSpawnPoints = new Transform[spawnContainer.transform.childCount];

        for (int i = 0; i < playerSpawnPoints.Length; i++)
        {
            playerSpawnPoints[i] = spawnContainer.transform.GetChild(i);
        }

        if (!runner.IsServer) return;

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            SpawnPlayer(runner, player);
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        playerSpawnPoints = null;
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
