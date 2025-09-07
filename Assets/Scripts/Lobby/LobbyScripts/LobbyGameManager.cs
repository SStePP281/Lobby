using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public enum GameName
{
    Run = 2,
    Jump = 3,
    Hide = 4,
}

public class LobbyGameManager : MonoBehaviour
{
    public static LobbyGameManager Instance { get; private set; }

    public Action OnCurrentMapChenged;
    public Action OnGameTypeChenged;

    [SerializeField] private LobbyMapDataListSO mapList;

    /*Client data*/
    private bool isClientReady;

    /*Host data*/
    private ReactiveVariable<LobbyMapDataSO> currentMapData;
    private ReactiveVariable<bool> isOnlineGame;
    private bool isAllReady;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        currentMapData = new ReactiveVariable<LobbyMapDataSO>(mapList.defaultMap, new LobbyMapDataSOComparer());
        currentMapData.OnValueChanged += CurrentMapData_OnValueChanged;

        isOnlineGame = new ReactiveVariable<bool>(true);
        isOnlineGame.OnValueChanged += IsOnlineGame_OnValueChanged;
    }

    private void Start()
    {
        LobbyServiceManager.Instance.OnLobbyDataUpdate += LobbyManager_OnLobbyDataUpdate;
        LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate += LobbyManager_OnLobbyGeneralDataUpdate;
    }

    private void LobbyManager_OnLobbyGeneralDataUpdate()
    {
        isAllReady = isClientReady = false;

        Debug.Log("Reset ready flags");

        //LobbyServiceManager.Instance.SetClientData(LobbyData.READY_FLAG_FIELD, false.ToString());
    }

    private void LobbyManager_OnLobbyDataUpdate()
    {
        UpdateMapData();
        UpdateReadyData();
        UpdateOnlineData();
    }

    private void UpdateMapData()
    {
        //Lobby lobby = LobbyServiceManager.Instance.GetLobby();

        //if (lobby.Data.ContainsKey(LobbyData.GAME_MAP_FIELD))
        //{
        //    currentMapData.Value = GetLobbyMapDataSOFromIndedx(int.Parse(lobby.Data[LobbyData.GAME_MAP_FIELD].Value));
        //}
    }

    private void UpdateReadyData()
    {
        //if (!LobbyServiceManager.Instance.IsMyLobby()) { return; }

        //List<Player> players = LobbyServiceManager.Instance.GetAllPlayers();

        bool isAllReadyNow = true;

        //foreach (var player in players)
        //{
        //    if (!bool.Parse(player.Data[LobbyData.READY_FLAG_FIELD].Value))
        //    {
        //        isAllReadyNow = false;
        //        break;
        //    }
        //}

        if (isAllReady && !isAllReadyNow)
        {
            Debug.Log("Stop search game");
        }
        else if (!isAllReady && isAllReadyNow)
        {
            Debug.Log($"Start search {(isOnlineGame.Value ? "Online" : "Local")} Game "  + currentMapData.Value.sceneViewName);
        }

        isAllReady = isAllReadyNow;
    }

    private void UpdateOnlineData()
    {
        //Lobby lobby = LobbyServiceManager.Instance.GetLobby();

        //f (lobby.Data.ContainsKey(LobbyData.GAME_ONLINE_TYPE_FIELD))
        //
        //   isOnlineGame.Value = bool.Parse(lobby.Data[LobbyData.GAME_ONLINE_TYPE_FIELD].Value);
        //
    }

    private void CurrentMapData_OnValueChanged(LobbyMapDataSO priviose, LobbyMapDataSO currnet) => OnCurrentMapChenged?.Invoke();

    private void IsOnlineGame_OnValueChanged(bool priviose, bool currnet) => OnGameTypeChenged?.Invoke();

    private void OnDestroy()
    {
        LobbyServiceManager.Instance.OnLobbyDataUpdate -= LobbyManager_OnLobbyDataUpdate;
        LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate -= LobbyManager_OnLobbyGeneralDataUpdate;

        currentMapData.OnValueChanged -= CurrentMapData_OnValueChanged;
        isOnlineGame.OnValueChanged -= IsOnlineGame_OnValueChanged;
    }

    public void SetClientReady(bool isReady)
    {
        isClientReady = isReady;

        //LobbyServiceManager.Instance.SetClientData(LobbyData.READY_FLAG_FIELD, isReady.ToString());
    }

    public void SetGameType(bool isOnlineGame)
    {
        this.isOnlineGame.Value = isOnlineGame;

        //LobbyServiceManager.Instance.SetLobbyData(LobbyData.GAME_ONLINE_TYPE_FIELD, isOnlineGame.ToString());
    }

    public void SetNewMap(LobbyMapDataSO newMap)
    {
        //LobbyServiceManager.Instance.SetLobbyData(LobbyData.GAME_MAP_FIELD, GetLobbyMapDataSOIndex(newMap).ToString());
    }

    public bool IsClientReady() => isClientReady;

    public bool IsOnlineGame() => isOnlineGame.Value;

    public int GetLobbyMapDataSOIndex(LobbyMapDataSO lobbyMapDataSO)
    {
        return mapList.mapList.IndexOf(lobbyMapDataSO);
    }

    public LobbyMapDataSO GetLobbyMapDataSOFromIndedx(int indedx)
    {
        return indedx >= 0 && indedx < mapList.mapList.Count ? mapList.mapList[indedx] : null;
    }

    public LobbyMapDataSO GetGefaultLobbyMapDataSO()
    {
        return mapList.defaultMap;
    }

    public LobbyMapDataSO GetCurrentLobbbyMapDataSo()
    {
        return currentMapData.Value;
    }

    public List<LobbyMapDataSO> GetLobbyMapDataList() => mapList.mapList;
}
