using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MenuNavigation : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject mainMenuPanel;
    public GameObject serversPanel;
    public GameObject createServerPanel;
    public GameObject joinServerPanel;

    [SerializeField] private PhotonManager photonManager;
    [SerializeField] private TMP_Text startText;

    private bool connectingToLobby;

    private void Start()
    {
        ShowStart();
    }

    private void Update()
    {
        if (startPanel.activeSelf && Keyboard.current.anyKey.wasPressedThisFrame && !connectingToLobby)
        {
            ConnectToLobby();
        }
    }

    private async void ConnectToLobby()
    {
        connectingToLobby = true;

        startText.text = "Connecting to the Lobby...";

        await photonManager.ConnectToServerLobby();

        ShowMainMenu();
    }

    public void ShowStart()
    {
        startPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        serversPanel.SetActive(false);
        createServerPanel.SetActive(false);
        joinServerPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        startPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        serversPanel.SetActive(false);
        createServerPanel.SetActive(false);
        joinServerPanel.SetActive(false);
    }

    public void ShowServers()
    {
        startPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        serversPanel.SetActive(true);
        createServerPanel.SetActive(false);
        joinServerPanel.SetActive(false);
    }

    public void ShowCreateServer()
    {
        startPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        serversPanel.SetActive(false);
        createServerPanel.SetActive(true);
        joinServerPanel.SetActive(false);
    }

    public void ShowJoinServer()
    {
        startPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        serversPanel.SetActive(false);
        createServerPanel.SetActive(false);
        joinServerPanel.SetActive(true);
    }
}
