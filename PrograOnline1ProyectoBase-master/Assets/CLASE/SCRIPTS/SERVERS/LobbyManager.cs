using Fusion;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject noSessionsFounding;
    [SerializeField] private GameObject sessionPrefab;
    [SerializeField] private Transform viewportContent;

    private void OnEnable()
    {
        PhotonManager.onSessionListUpdated += OnSessionListUpdated;

        OnSessionListUpdated();
    }

    private void OnDisable()
    {
        PhotonManager.onSessionListUpdated -= OnSessionListUpdated;
    }

    public void OnSessionListUpdated()
    {
        foreach (Transform child in viewportContent)
        {
            if (child.GetComponent<SessionEntry>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        noSessionsFounding.SetActive(PhotonManager.sessionList.Count == 0);

        if (PhotonManager.sessionList.Count == 0)
        {
            noSessionsFounding.SetActive(true);
        }
        else
        {
            for(int i = 0; i <  PhotonManager.sessionList.Count; i++)
            {
                GameObject session = Instantiate(sessionPrefab, viewportContent);

                ServerDataStruct serverData = new ServerDataStruct();
                serverData.serverName = PhotonManager.sessionList[i].Name;
                serverData.currentPlayers = PhotonManager.sessionList[i].PlayerCount;
                serverData.maxPlayers = PhotonManager.sessionList[i].MaxPlayers;
                serverData.mapName = PhotonManager.sessionList[i].Properties["MapName"];
                serverData.gameMode = PhotonManager.sessionList[i].Properties["GameMode"];


                session.GetComponent<SessionEntry>().SetInfo(serverData);
            }
        }
    }

}
