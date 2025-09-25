using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class LobbySpawnManager : MonoBehaviour
{
    [Inject] private ICharacterFactory characterFactory;
    [Inject] private ILobbyPlayerSetupFactory playerSetupFactory;

    [SerializeField] private List<Transform> spawnPointList;

    private Dictionary<string, LobbyPlayerSetup> lobbyPlayerDict;
    private ObjectPool<LobbyPlayerSetup> objectPool;

    int currentSpawnPointIndex;

    private void Awake()
    {
        lobbyPlayerDict = new Dictionary<string, LobbyPlayerSetup>();
        objectPool = new ObjectPool<LobbyPlayerSetup>((Transform a) => { return playerSetupFactory.CreatePlayerSetup(a); }, null);

        AuthenticationServiceManager.Instance.OnSignIn += AuthenticationServiceManager_OnSignIn;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
        HashDataManager.Instance.OnUserDataChanged += HashDataManager_OnUserDataChanged;
        LobbyServiceManager.Instance.OnLobbyDataUpdate += LobbyServiceManager_OnLobbyDataUpdate;
        LobbyServiceManager.Instance.OnLobbyClosed += LobbyServiceManager_OnLobbyClosed;
    }

    private LobbyPlayerSetup CreatePlayerSetup(LobbyUserData lobbyUserData)
    {
        LobbyPlayerSetup lobbyPlayerSetup = objectPool.GetElement(spawnPointList[currentSpawnPointIndex]);
        currentSpawnPointIndex++;

        lobbyPlayerDict[lobbyUserData.UserID] = lobbyPlayerSetup.Initialize(lobbyUserData, characterFactory.CreateCharacter(lobbyUserData.CharacterIndex, lobbyPlayerSetup.GetModelContainer()));

        return lobbyPlayerSetup;
    }

    private void UpdatePlayerCharater(LobbyUserData lobbyUserData)
    {
        LobbyPlayerSetup lobbyPlayerSetup = lobbyPlayerDict[lobbyUserData.UserID];

        lobbyPlayerSetup.SetNickname(lobbyUserData.Nickname);
        lobbyPlayerSetup.SetHostFlag(lobbyUserData.IsHost);
        lobbyPlayerSetup.SetReadyFlag(lobbyUserData.IsReady);

        if (lobbyPlayerSetup.CharacterIndex == lobbyUserData.CharacterIndex) { return; }

        lobbyPlayerSetup.SetModel(lobbyUserData.CharacterIndex, characterFactory.CreateCharacter(lobbyUserData.CharacterIndex, lobbyPlayerSetup.GetModelContainer()));
    }

    private void DisposePlayerSetup(string userID)
    {
        LobbyPlayerSetup lobbyPlayerSetup = lobbyPlayerDict[userID];
        objectPool.Hide(lobbyPlayerSetup);
        lobbyPlayerDict.Remove(userID);
        currentSpawnPointIndex--;
    }

    private void RefreshData()
    {
        List<LobbyUserData> lobbyUserDataList = LobbyServiceManager.Instance.GetLobbyUserList();

        if (lobbyUserDataList == null) { return; }

        foreach (LobbyUserData lobbyUserData in lobbyUserDataList)
        {
            if (!lobbyPlayerDict.ContainsKey(lobbyUserData.UserID))
            {
                lobbyPlayerDict[lobbyUserData.UserID] = CreatePlayerSetup(lobbyUserData);
                continue;
            }

            UpdatePlayerCharater(lobbyUserData);
        }

        List<string> playersToRemove = new List<string>();

        foreach (KeyValuePair<string, LobbyPlayerSetup> lobbyPlayers in lobbyPlayerDict)
        {
            if (!lobbyUserDataList.Any(p => p.UserID == lobbyPlayers.Key))
            {
                playersToRemove.Add(lobbyPlayers.Key);
            }
        }

        if (playersToRemove.Count == 0) { return; }

        foreach (string playerId in playersToRemove)
        {
            DisposePlayerSetup(playerId);
        }

        RefreshData();
    }

    private void LobbyServiceManager_OnLobbyDataUpdate() => RefreshData();

    private void LobbyServiceManager_OnLobbyClosed()
    {
        RefreshData();

        HashDataManager_OnUserDataChanged();
    }

    private void HashDataManager_OnUserDataChanged()
    {
        if (LobbyServiceManager.Instance.IsLobbyCreated() || !AuthenticationServiceManager.Instance.IsSignedIn) { return; }

        if (!lobbyPlayerDict.ContainsKey(AuthenticationServiceManager.Instance.GetUserId()))
        {
            AuthenticationServiceManager_OnSignIn();
        }

        UpdatePlayerCharater(new LobbyUserData
        {
            UserID = AuthenticationServiceManager.Instance.GetUserId(),
            Nickname = string.Empty,
            CharacterIndex = HashDataManager.Instance.GetMyCharacterData().CharacterIndex,
            IsHost = false,
            IsReady = false
        });
    }

    private void AuthenticationServiceManager_OnSignIn()
    {
        CreatePlayerSetup(new LobbyUserData
        {
            UserID = AuthenticationServiceManager.Instance.GetUserId(),
            Nickname = string.Empty,
            CharacterIndex = HashDataManager.Instance.GetMyCharacterData().CharacterIndex,
            IsHost = false,
            IsReady = false
        });
    }

    private void AuthenticationServiceManager_OnSignOut()
    {
        objectPool.Clear();
        lobbyPlayerDict.Clear();

        currentSpawnPointIndex = 0;
    }

    private void OnDestroy()
    {
        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignIn -= AuthenticationServiceManager_OnSignIn;
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }

        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnUserDataChanged -= HashDataManager_OnUserDataChanged;
        }

        if (LobbyServiceManager.Instance != null)
        {
            LobbyServiceManager.Instance.OnLobbyDataUpdate -= LobbyServiceManager_OnLobbyDataUpdate;
            LobbyServiceManager.Instance.OnLobbyClosed -= LobbyServiceManager_OnLobbyClosed;
        }

        objectPool.Clear();
        lobbyPlayerDict.Clear();
    }
}
