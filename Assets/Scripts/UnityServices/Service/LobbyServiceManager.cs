using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Services.Friends;

public class LobbyServiceManager : IDisposable
{
    /*Const*/
    private const float HEARTBEAT_DELAY = 15.0f;
    private const float UPDATE_LOBBY_DATA_DELAY = 1.1f;
    private const float MAX_UPDATE_REQUEST_INTERVAL = 5.0f;
    private const int MAX_UPDATE_REQUEST_COUNT = 5;

    private static bool isDisposing;
    private static LobbyServiceManager instance;
    public static LobbyServiceManager Instance
    {
        get
        {
            if (isDisposing) { return null; }

            if (instance == null)
            {
                instance = new LobbyServiceManager();
            }
            return instance;
        }
    }

    /*Action*/
    public Action OnLobbyGenerealDataUpdate;
    public Action OnLobbyDataUpdate;

    /*General param*/
    private ReactiveVariable<Lobby> lobby;

    /*Async param*/
    private readonly SemaphoreSlim lobbySemaphore = new SemaphoreSlim(1, 1);
    private CancellationTokenSource loopsCts;
    private bool isCreatingLobby;

    /*Lobby data param*/
    private string lobbyPassword;
    private string clientName;

    /*Update param*/
    private Queue<DateTime> lobbyUpdateTimestamps;
    private Queue<DateTime> clientUpdateTimestamps;
    private TimeSpan lastUpdateTime;

    private LobbyServiceManager()
    {
        loopsCts = new CancellationTokenSource();

        lobbyUpdateTimestamps = new Queue<DateTime>();
        clientUpdateTimestamps = new Queue<DateTime>();
        lastUpdateTime = TimeSpan.FromSeconds(MAX_UPDATE_REQUEST_INTERVAL);

        lobby = new ReactiveVariable<Lobby>(null, new LobbyEqualsComparer());
        //lobby.OnValueChanged += Lobby_OnValueChanged;

        _ = FriendsService.Instance.InitializeAsync();
    }

    public bool CreateLobby(Action<bool, string> callback = null)
    {
        if (lobby.Value != null || isCreatingLobby) { return false; }

        _ = CreateLobbyAsync(callback);

        return true;
    }

    public bool JoinLobbyByID(string lobbyId, Action<bool, string> callback = null)
    {
        if (string.IsNullOrEmpty(lobbyId)) { return false; }

        _ = JoinLobbyByIdAsync(lobbyId, callback);

        return true;
    }

    public bool LeaveFromLobby()
    {
        if (lobby.Value == null) { return false; }

        _ = LeaveFromLobbyAsync();

        return true;
    }

    public bool KickClient(string clientID)
    {
        if (!IsMyLobby() || string.IsNullOrEmpty(clientID) || clientID == AuthenticationServiceManager.Instance.GetUserId()) { return false; }

        _ = KickClientFromLobbyAsync(clientID);

        return true;
    }

    public bool SetNewLobbyHost(string clientID)
    {
        if (!IsMyLobby() || string.IsNullOrEmpty(clientID) || clientID == AuthenticationServiceManager.Instance.GetUserId()) { return false; }

        _ = MigrateLobbyHostAsync(clientID);

        return true;
    }

    private void StartLobbyLoops()
    {
        loopsCts?.Cancel();
        loopsCts = new CancellationTokenSource();

        _ = UpdateLobbyData(loopsCts.Token);
        _ = SendHeartbeatLobby(loopsCts.Token);
    }

    private void StopLobbyLoops()
    {
        loopsCts?.Cancel();
    }

    private async Task CreateLobbyAsync(Action<bool, string> callback = null)
    {
        await lobbySemaphore.WaitAsync();
        try
        {
            if (isCreatingLobby) { return; }
            isCreatingLobby = true;

            Player player = GetClient();
            lobbyPassword = LobbyData.DEFAULT_LOBBY_PASSWORD;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = player,
                Password = lobbyPassword,
                Data = new Dictionary<string, DataObject>
                {
                    {LobbyData.GAME_ONLINE_TYPE_FIELD, new DataObject(DataObject.VisibilityOptions.Public, LobbyGameManager.Instance.IsOnlineGame().ToString()) },
                    {LobbyData.GAME_MAP_FIELD, new DataObject(DataObject.VisibilityOptions.Public, LobbyGameManager.Instance.GetLobbyMapDataSOIndex(LobbyGameManager.Instance.GetCurrentLobbbyMapDataSo()).ToString()) }
                }
            };

            try
            {
                Lobby createdLobby = await LobbyService.Instance.CreateLobbyAsync("NON", LobbyData.maxLobbySize, createLobbyOptions);
                lobby.Value = createdLobby;

                Debug.Log("Create lobby with code: " + lobby.Value.LobbyCode);

                StartLobbyLoops();

                callback?.Invoke(true, string.Empty);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);

                callback?.Invoke(false, e.Message);
            }
            catch (Exception e)
            {
                Debug.Log(e);

                callback?.Invoke(false, e.Message);
            }
        }
        finally
        {
            isCreatingLobby = false;
            lobbySemaphore.Release();
        }
    }

    private async Task JoinLobbyByIdAsync(string lobbyId, Action<bool, string> callback = null)
    {
        if (lobby.Value != null)
        {
            await LeaveFromLobbyAsync();
        }

        await lobbySemaphore.WaitAsync();
        try
        {
            Player player = GetClient();
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = player
            };

            try
            {
                await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);

                Debug.Log($"Join to lobby {lobbyId}");

                callback?.Invoke(true, string.Empty);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);

                callback?.Invoke(false, e.Message);
            }
        }
        finally
        {
            lobbySemaphore.Release();
        }
    }

    private async Task LeaveFromLobbyAsync()
    {
        await lobbySemaphore.WaitAsync();
        try
        {
            if (IsMyLobby())
            {
                await MigrateLobbyHostAsync(lobby.Value.Players[1].Id);
            }

            try
            {
                string lobbyId = lobby.Value.Id;
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, AuthenticationServiceManager.Instance.GetUserId());

                Debug.Log($"Leave from lobby {lobbyId}");

                lobby.Value = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        finally
        {
            lobbySemaphore.Release();
        }
    }

    private async Task MigrateLobbyHostAsync(string newHostId)
    {
        if (!lobby.Value.Players.Any(p => p.Id == newHostId)) { return; }

        await lobbySemaphore.WaitAsync();
        try
        {
            try
            {
                Lobby updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Value.Id, new UpdateLobbyOptions
                {
                    HostId = newHostId,
                });
                lobby.Value = updatedLobby;

                Debug.Log("Set new lobby host: " + lobby.Value.HostId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        finally
        {
            lobbySemaphore.Release();
        }
    }

    private async Task KickClientFromLobbyAsync(string clientId)
    {
        await lobbySemaphore.WaitAsync();
        try
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(lobby.Value.Id, clientId);

                Debug.Log("Kick client from lobby: " + clientId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        finally
        {
            lobbySemaphore.Release();
        }
    }

    private async Task SendHeartbeatLobby(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(HEARTBEAT_DELAY), ct);

                if (ct.IsCancellationRequested) { break; }

                if (!IsMyLobby()) { continue; }

                await lobbySemaphore.WaitAsync(ct);
                try
                {
                    if (lobby.Value != null)
                    {
                        await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Value.Id);
                    }
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    lobbySemaphore.Release();
                }
            }
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            StopLobbyLoops();
        }
    }

    private async Task UpdateLobbyData(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(UPDATE_LOBBY_DATA_DELAY), ct);

                if (ct.IsCancellationRequested) { break; }

                if (lobby.Value == null) { break; }

                await lobbySemaphore.WaitAsync(ct);
                try
                {
                    Lobby updatedLobby = await LobbyService.Instance.GetLobbyAsync(lobby.Value.Id);

                    if (updatedLobby != null)
                    {
                        string userId = AuthenticationServiceManager.Instance.GetUserId();
                        bool iAmStillIn = updatedLobby.Players.Any(p => p.Id == userId);

                        if (!iAmStillIn)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                    lobby.Value = updatedLobby;
                    OnLobbyDataUpdate?.Invoke();
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);

                    break;
                }
                finally
                {
                    lobbySemaphore.Release();
                }
            }
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            StopLobbyLoops();
        }
    }

    private Player GetClient()
    {
        return new Player(AuthenticationServiceManager.Instance.GetUserId(), null, new Dictionary<string, PlayerDataObject>
        {
            { LobbyData.PLAYER_NAME_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationServiceManager.Instance.GetPlayerName())},
            { LobbyData.PLAYER_CHARACTER_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, LobbyCharacterManager.Instance.GetCurrentClientCharacterIndex().ToString()) },
            { LobbyData.READY_FLAG_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, LobbyGameManager.Instance.IsClientReady().ToString())}
        });
    }

    public bool IsLobbyCreated() => lobby.Value != null;

    public string GetLobbyId() => lobby.Value != null ? lobby.Value.Id : string.Empty;

    public bool IsMyLobby() => lobby.Value != null && lobby.Value.HostId == AuthenticationServiceManager.Instance.GetUserId();

    public void Dispose()
    {
        isDisposing = true;

        instance = null;

        loopsCts?.Cancel();

        //lobby.OnValueChanged -= Lobby_OnValueChanged;
    }
}

//private bool CanSendRequest(Queue<DateTime> timestamps)
//{
//    var now = DateTime.UtcNow;

//    while (timestamps.Count > 0 && (now - timestamps.Peek()) > lastUpdateTime)
//    {
//        timestamps.Dequeue();
//    }

//    if (timestamps.Count >= MAX_UPDATE_REQUEST_COUNT)
//    {
//        return false;
//    }

//    timestamps.Enqueue(now);
//    return true;
//}

//public void SetClientData(string dataField, string data) => _ = UpdateClientData(dataField, data);

//public void SetLobbyData(string dataField, string data) => _ = UpdateLobbyData(dataField, data);

///*Invoke func*/
//private void Lobby_OnValueChanged(Lobby priviouse, Lobby currnet) => OnLobbyGenerealDataUpdate?.Invoke();

//private async Task UpdateClientName(string newClientName)
//{
//    if (string.IsNullOrEmpty(newClientName)) { newClientName = "Guest" + AuthenticationService.Instance.PlayerId.Substring(0, 5); }

//    if (newClientName == clientName) { return; }

//    await lobbySemaphore.WaitAsync();
//    try
//    {
//        UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions
//        {
//            Data = new Dictionary<string, PlayerDataObject>
//            {
//                {LobbyData.PLAYER_NAME_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, newClientName)}
//            }
//        };

//        try
//        {
//            Lobby updatedLobby = await LobbyService.Instance.UpdatePlayerAsync(lobby.Value.Id, AuthenticationServiceManager.Instance.GetUserId(), updatePlayerOptions);
//            lobby.Value = updatedLobby;

//            clientName = newClientName;

//            Debug.Log("Set new name in lobby: " + clientName);
//        }
//        catch (LobbyServiceException e)
//        {
//            Debug.Log(e);
//        }
//    }
//    finally
//    {
//        lobbySemaphore.Release();
//    }
//}

//public async Task UpdateLobbyData(string dataField, string data)
//{
//    if (string.IsNullOrEmpty(dataField) || string.IsNullOrEmpty(data)) { return; }
//    if (lobby.Value == null) { return; }  
//    if (!CanSendRequest(clientUpdateTimestamps))
//    {
//        Debug.Log("Lobby update request skipped due to rate limit.");
//        return;
//    }

//    await lobbySemaphore.WaitAsync();
//    try
//    {
//        try
//        {
//            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
//            {
//                Data = new Dictionary<string, DataObject>
//                {
//                    {dataField, new DataObject(DataObject.VisibilityOptions.Public, data)}
//                }
//            };

//            Lobby updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Value.Id, updateLobbyOptions);
//            lobby.Value = updatedLobby;

//            Debug.Log($"Update lobby {dataField} with {data}");
//        }
//        catch (LobbyServiceException e)
//        {
//            Debug.Log(e);
//        }
//    }
//    finally
//    {
//        lobbySemaphore.Release();
//    }
//}

//public async Task UpdateClientData(string dataField, string data)
//{
//    if (string.IsNullOrEmpty(dataField) || string.IsNullOrEmpty(data)) { return; }
//    if (lobby.Value == null) { return; }
//    if (!CanSendRequest(lobbyUpdateTimestamps)) 
//    {
//        Debug.Log("Lobby update request skipped due to rate limit.");
//        return;
//    }

//    await lobbySemaphore.WaitAsync();
//    try
//    {
//        try
//        {
//            UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions
//            {
//                Data = new Dictionary<string, PlayerDataObject>
//                {
//                    {dataField, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, data)}
//                }
//            };

//            Lobby updatedLobby = await LobbyService.Instance.UpdatePlayerAsync(lobby.Value.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
//            lobby.Value = updatedLobby;

//            Debug.Log($"Update client {dataField} with {data}");
//        }
//        catch (LobbyServiceException e)
//        {
//            Debug.Log(e);
//        }
//    }
//    finally
//    {
//        lobbySemaphore.Release();
//    }
//}
