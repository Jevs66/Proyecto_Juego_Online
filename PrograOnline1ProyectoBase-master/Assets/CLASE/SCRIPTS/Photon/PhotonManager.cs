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
    private NetworkRunner runner;

    [SerializeField] private UnityEvent onPlayerJoined;

    [SerializeField] private NetworkPrefabRef playerPrefab;

    [SerializeField] private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();

    private void Awake()
    {
        runner = GetComponent<NetworkRunner>();
        runner.ProvideInput = true;

    }

    private void Start()
    {
        runner.AddCallbacks(this);
    }

    private async Task StartGame(GameMode mode)
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
            CustomLobbyName = "Lobby de pruebas para clase"
        });
    }

    public void StartGameAsHost()
    {
        Task startGame = StartGame(GameMode.Host);
    }

    public void StartGameAsClient()
    {
        Task startGame = StartGame(GameMode.Client);
    }

    #region Metodos de Photon

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);

        if(runner.IsServer)
            players.Add(player, runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player));

        onPlayerJoined?.Invoke();
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
            isShooting = InputManager.Instance.IsShooting()
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

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

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
