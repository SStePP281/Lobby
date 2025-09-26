using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using ServiceRequest;

public class LobbyServiceManager : IDisposable
{
    public event Action OnLobbyDataUpdate;
    public event Action OnLobbyGeneralDataUpdate;
    public event Action OnLobbyCreated;
    public event Action OnLobbyClosed;

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

    private ILobbyUserDataCreator userDataCreator;

    private ReactiveVariable<Lobby> lobby;

    private readonly SemaphoreSlim lobbySemaphore = new SemaphoreSlim(1, 1);
    private CancellationTokenSource loopsCts;
    private bool isCreatingLobby;

    private CancellationTokenSource aloneTimerCts;

    private LobbyServiceManager()
    {
        lobby = new ReactiveVariable<Lobby>(null, new LobbyEqualityComparer());
        lobby.OnValueChanged += Lobby_OnValueChanged;

        AuthenticationServiceManager.Instance.OnSignOutWait += AuthenticationServiceManager_OnSignOut;

        loopsCts = new CancellationTokenSource();
    }

    private void Lobby_OnValueChanged(Lobby privious, Lobby current)
    {
        OnLobbyGeneralDataUpdate?.Invoke();

        if (privious == null && current != null)
        {
            OnLobbyCreated?.Invoke();
            return;
        }
        else if (current == null && privious != null)
        {
            OnLobbyClosed?.Invoke();
            return;
        }

        if (current.Players.Count != privious.Players.Count && current.Players.Count == 1)
        {
            if (current.Players.Count == 1)
            {
                _ = StartAloneTimer();
            }
            else if (privious.Players.Count == 1)
            {
                ResetAloneTimer();
            }
        }
    }

    public void SetLobbyUserDataCreator(ILobbyUserDataCreator creator) => userDataCreator = creator;

    public Request CreateLobby()
    {
        Request request = new Request(async () =>
        {
            if (lobby.Value != null || isCreatingLobby)
            {
                throw new Exception("A lobby is already created or currently being created.");
            }

            await lobbySemaphore.WaitAsync();
            try
            {
                if (isCreatingLobby) { return; }
                isCreatingLobby = true;

                Player player = GetClient();

                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                {
                    Player = player,
                    Password = LobbyServiceData.DEFAULT_LOBBY_PASSWORD,
                    Data = new Dictionary<string, DataObject> { }
                };

                Lobby createdLobby = await LobbyService.Instance.CreateLobbyAsync("NON", LobbyServiceData.MAX_LOBBY_SIZE, createLobbyOptions);
                lobby.Value = createdLobby;
            }
            finally
            {
                isCreatingLobby = false;
                lobbySemaphore.Release();
            }

        })
        .OnSuccessComplete(StartLobbyLoops);

        return request;
    }

    public Request JoinLobbyByID(string targetLobbyID)
    {
        Request request = new Request(async () =>
        {
            if (string.IsNullOrEmpty(targetLobbyID))
            {
                throw new Exception("Lobby ID cannot be empty or null.");
            }

            if (lobby.Value != null)
            {
                await LeaveFromLobby().StartAndWait();
            }

            await lobbySemaphore.WaitAsync();
            try
            {
                Player player = GetClient();

                JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
                {
                    Player = player,
                    Password = LobbyServiceData.DEFAULT_LOBBY_PASSWORD
                };

                Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(targetLobbyID, joinLobbyByIdOptions);
                lobby.Value = joinedLobby;

                Debug.Log($"Join to lobby {targetLobbyID}");
            }
            finally
            {
                lobbySemaphore.Release();
            }
        }).OnSuccessComplete(StartLobbyLoops);

        return request;
    }

    public Request LeaveFromLobby()
    {
        Request request = new Request(async () =>
        {
            if (lobby.Value == null)
            {
                throw new Exception("You are not in a lobby.");
            }

            if (IsMyLobby())
            {
                await MigrateLobbyHost(lobby.Value.Players[1].Id).StartAndWait();
            }

            await lobbySemaphore.WaitAsync();
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(lobby.Value.Id, AuthenticationServiceManager.Instance.GetUserId());

                lobby.Value = null;
            }
            finally
            {
                lobbySemaphore.Release();
            }
        }).OnSuccessComplete(StopLobbyLoops);

        return request;
    }

    public Request KickClientFromLobby(string clientID)
    {
        Request request = new Request(async () =>
        {
            if (!IsMyLobby())
            {
                throw new Exception("Only the lobby host can kick players.");
            }

            if (string.IsNullOrEmpty(clientID))
            {
                throw new Exception("Player ID to kick is invalid.");
            }

            if (clientID == AuthenticationServiceManager.Instance.GetUserId())
            {
                throw new Exception("You cannot kick yourself.");
            }

            await LobbyService.Instance.RemovePlayerAsync(lobby.Value.Id, clientID);

            Debug.Log("Kick client from lobby: " + clientID);
        });

        return request;
    }

    public Request MigrateLobbyHost(string clientID)
    {
        Request request = new Request(async () =>
        {
            if (!IsMyLobby())
            {
                throw new Exception("Only the current host can transfer host privileges.");
            }

            if (string.IsNullOrEmpty(clientID))
            {
                throw new Exception("New host ID is invalid.");
            }

            if (clientID == AuthenticationServiceManager.Instance.GetUserId())
            {
                throw new Exception("You are already the host.");
            }

            if (!lobby.Value.Players.Any(p => p.Id == clientID))
            {
                throw new Exception("The selected player is not in this lobby.");
            }

            await lobbySemaphore.WaitAsync();
            try
            {
                UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
                {
                    HostId = clientID
                };

                Lobby updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Value.Id, updateLobbyOptions);
                lobby.Value = updatedLobby;

                Debug.Log("Set new lobby host: " + lobby.Value.HostId);
            }
            finally
            {
                lobbySemaphore.Release();
            }
        });

        return request;
    }

    public Request UpdatePlayerData(string dataField, string data, bool isPublic = true)
    {
        Request request = new Request(async () =>
        {
            if (lobby.Value == null)
            {
                throw new Exception("You must be in a lobby to update player data.");
            }

            PlayerDataObject.VisibilityOptions visibilityOptions = isPublic ? PlayerDataObject.VisibilityOptions.Public : PlayerDataObject.VisibilityOptions.Private;

            UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { dataField, new PlayerDataObject(visibilityOptions, data) }
                }
            };

            await lobbySemaphore.WaitAsync();
            try
            {
                Lobby updatedLobby = await LobbyService.Instance.UpdatePlayerAsync(lobby.Value.Id, AuthenticationServiceManager.Instance.GetUserId(), updatePlayerOptions);
                lobby.Value = updatedLobby;
            }
            finally
            {
                lobbySemaphore.Release();
            }
        });

        return request;
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

    private async Task StartAloneTimer()
    {
        if (aloneTimerCts != null && !aloneTimerCts.IsCancellationRequested)
            return;

        ResetAloneTimer();

        aloneTimerCts = CancellationTokenSource.CreateLinkedTokenSource(loopsCts.Token);

        await Task.Delay(TimeSpan.FromSeconds(LobbyServiceData.LOBBY_AUTO_DESTROY_DELAY_IF_ALONE), aloneTimerCts.Token);

        if (lobby.Value == null) { return; }

        await lobbySemaphore.WaitAsync();
        try
        {
            if (lobby.Value.Players.Count == 1)
            {
                if (IsMyLobby())
                {
                    await LobbyService.Instance.DeleteLobbyAsync(lobby.Value.Id);
                }

                lobby.Value = null;
            }
        }
        catch (Exception)
        {

        }
        finally
        {
            lobbySemaphore.Release();
            aloneTimerCts?.Dispose();
            StopLobbyLoops();
            aloneTimerCts = null;
        }
    }

    private void ResetAloneTimer()
    {
        aloneTimerCts?.Cancel();
        aloneTimerCts?.Dispose();
        aloneTimerCts = null;
    }

    private async Task SendHeartbeatLobby(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(LobbyServiceData.HEARTBEAT_DELAY), ct);

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
        catch (Exception)
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
                await Task.Delay(TimeSpan.FromSeconds(LobbyServiceData.UPDATE_LOBBY_DATA_DELAY), ct);

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
                            lobby.Value = null;
                            break;
                        }
                    }
                    else
                    {
                        lobby.Value = null;
                        break;
                    }

                    lobby.Value = updatedLobby;

                    OnLobbyDataUpdate?.Invoke();
                }
                catch (Exception)
                {
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
            { LobbyServiceData.PLAYER_NICKNAME_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, HashDataManager.Instance.GetMyNickname()) },
            { LobbyServiceData.PLAYER_CHARACTER_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, HashDataManager.Instance.GetMyCharacterData().CharacterIndex.ToString()) },
            { LobbyServiceData.PLAYER_READY_FLAG_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, false.ToString())}
        });
    }

    public bool IsLobbyCreated() => lobby.Value != null;

    public string GetLobbyId() => lobby.Value != null ? lobby.Value.Id : string.Empty;

    public bool IsMyLobby() => GetHostID() == AuthenticationServiceManager.Instance.GetUserId();

    public bool LobbyContainsUser(string userID)
    {
        if (lobby.Value == null) { return false; }

        foreach (Player player in lobby.Value.Players)
        {
            if (player.Id == userID) { return true; }
        }

        return false;
    }

    public string GetHostID() => lobby.Value != null ? lobby.Value.HostId : string.Empty;

    public int GetLobbyUserCount() => lobby.Value == null ? 1 : lobby.Value.Players.Count;

    public List<LobbyUserData> GetLobbyUserList()
    {
        List<LobbyUserData> list = new List<LobbyUserData>();

        if (lobby.Value == null) { return list; }

        foreach (Player player in lobby.Value.Players)
        {
            list.Add(userDataCreator.GetUserData(player));
        }

        return list;
    }

    private async Task AuthenticationServiceManager_OnSignOut() => await LeaveFromLobby().StartAndWait();

    public void Dispose()
    {
        isDisposing = true;

        instance = null;

        loopsCts?.Cancel();

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignOutWait -= AuthenticationServiceManager_OnSignOut;
        }
    }
}