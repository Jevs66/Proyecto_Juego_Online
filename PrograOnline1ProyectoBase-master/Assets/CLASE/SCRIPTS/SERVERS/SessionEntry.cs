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
        currentPlayers.text = serverData.currentPlayers + "/" + serverData.maxPlayers;
    }

}
