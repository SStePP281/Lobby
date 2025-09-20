//using System.Collections.Generic;
//using System.Linq;
//using Unity.Services.Lobbies.Models;
//using UnityEngine;


//public class LobbyCharacterManager : MonoBehaviour
//{
//    public static LobbyCharacterManager Instance { get; private set; }

//    [SerializeField] private LobbyPlayerDataListSO characterList;
//    [SerializeField] private GameObject prefabPlayerSetup;
//    [SerializeField] private List<Transform> spawnPoint;

//    private Dictionary<string, LobbyPlayerSetup> playerSetupDict;

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(this);
//            return;
//        }

//        playerSetupDict = new Dictionary<string, LobbyPlayerSetup>();
//    }

//    private void Start()
//    {
//        LobbyServiceManager.Instance.OnLobbyDataUpdate += LobbyManager_OnLobbyDataUpdate;
//        LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate += LobbyManager_OnLobbyGenerealDataUpdate;
//    }

//    private void LobbyManager_OnLobbyDataUpdate()
//    {
//        List<Player> currentPlayers = LobbyServiceManager.Instance.GetAllPlayers();

//        foreach (Player player in currentPlayers)
//        {
//            if (playerSetupDict.ContainsKey(player.Id))
//            {
//                playerSetupDict[player.Id].UpdatePlalaerData(player);
//            }
//            else
//            {
//                if (playerSetupDict.Count >= spawnPoint.Count)
//                {
//                    Debug.LogWarning("Lobby is full, but trying to add more players.");
//                    continue;
//                }

//                GameObject t = Instantiate(prefabPlayerSetup, spawnPoint[playerSetupDict.Count]);

//                if (t.TryGetComponent(out LobbyPlayerSetup lobbyPlayerSetup))
//                {
//                    lobbyPlayerSetup.UpdatePlalaerData(player);
//                    string hostId = LobbyServiceManager.Instance.GetHostID();
//                    lobbyPlayerSetup.SetHostFlag(player.Id == hostId);
//                    playerSetupDict[player.Id] = lobbyPlayerSetup;

//                    Debug.Log("Add new client: " + player.Id);
//                }
//            }
//        }

//        List<string> playersToRemove = new List<string>();

//        foreach (var lobbyPlayers in playerSetupDict)
//        {
//            if (!currentPlayers.Any(p => p.Id == lobbyPlayers.Key))
//            {
//                playersToRemove.Add(lobbyPlayers.Key);
//            }
//        }

//        foreach (string playerId in playersToRemove)
//        {
//            Destroy(playerSetupDict[playerId].gameObject);
//            playerSetupDict.Remove(playerId);

//            Debug.Log("Remove client: " + playerId);
//        }
//    }

//    private void LobbyManager_OnLobbyGenerealDataUpdate()
//    {
//        string hostID = LobbyServiceManager.Instance.GetHostID();

//        foreach (var pair in playerSetupDict)
//        {
//            pair.Value.SetHostFlag(hostID == pair.Key);
//        }

//        Debug.Log("Update host data");
//    }

//    private void OnDestroy()
//    {
//        LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate -= LobbyManager_OnLobbyDataUpdate;
//        LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate -= LobbyManager_OnLobbyGenerealDataUpdate;
//    }

//    public void SetNewCharacter(LobbyPlayerDataSO lobbyPlayerDataSO)
//    {
//        int newIndex = GetLobbyPlayerDataSOFIndex(lobbyPlayerDataSO);

//        if (newIndex != GetCurrentClientCharacterIndex())
//        {
//            playerSetupDict[LobbyServiceManager.Instance.GetMyClientId()].UpdatePlayerCharacterData(lobbyPlayerDataSO);

//            LobbyServiceManager.Instance.SetClientData(LobbyData.PLAYER_CHARACTER_FIELD, newIndex.ToString());
//        }
//    }

//    public LobbyPlayerDataSO GetLobbyPlayerDataSOFromIndex(int index)
//    {
//        return index >= 0 && index < characterList.characterList.Count ? characterList.characterList[index] : null;
//    }

//    public int GetLobbyPlayerDataSOFIndex(LobbyPlayerDataSO playerDataSO)
//    {
//        return characterList.characterList.IndexOf(playerDataSO);
//    }

//    public LobbyPlayerDataSO GetDefaultLobbyPlayerDataSO()
//    {
//        return characterList.defaultCharacter;
//    }

//    public LobbyPlayerDataListSO GetLobbyPlayerDataListSO()
//    {
//        return characterList;
//    }

//    public int GetCurrentClientCharacterIndex()
//    {
//        if (playerSetupDict.ContainsKey(LobbyServiceManager.Instance.GetMyClientId()))
//        {
//            return GetLobbyPlayerDataSOFIndex(playerSetupDict[LobbyServiceManager.Instance.GetMyClientId()].GetLobbyPlayerDataSO());
//        }
//        else
//        {
//            return GetLobbyPlayerDataSOFIndex(GetDefaultLobbyPlayerDataSO());
//        }
//        return 0;
//    }
//}
