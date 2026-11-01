using UnityEngine;
using TMPro;

public class SessionEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text serverName;
    [SerializeField] private TMP_Text mapName;
    [SerializeField] private TMP_Text gameMode;
    [SerializeField] private TMP_Text currentPlayers;

    public void SetInfo(ServerDataStruct serverData)
    {
        serverName.text = serverData.serverName;
        mapName.text = serverData.mapName;
        gameMode.text = serverData.gameMode;
        currentPlayers.text = serverData.currentPlayers + "/" + serverData.maxPlayers;
    }

    public void JoinGame()
    {
        PhotonManager.Instance.StartGameAsClient(serverName.text);
    }

}
