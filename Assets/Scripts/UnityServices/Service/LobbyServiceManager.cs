using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Services.Friends;
using ServiceRequest;

public class LobbyServiceManager : IDisposable
{
    public event Action OnLobbyDataUpdate;

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

    private Lobby lobby;

    private readonly SemaphoreSlim lobbySemaphore = new SemaphoreSlim(1, 1);
    private CancellationTokenSource loopsCts;
    private bool isCreatingLobby;

    private LobbyServiceManager()
    {
        loopsCts = new CancellationTokenSource();

        HashDataManager.Instance.OnUserDataChanged += HashDataManager_OnUserDataChanged;
    }

    public void SetLobbyUserDataCreator(ILobbyUserDataCreator creator) => userDataCreator = creator;

    public Request CreateLobby()
    {
        Request request = new Request(async () =>
        {
            if (lobby != null || isCreatingLobby)
            {
                throw new Exception("Lobby all ready created or creating now");
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

                Lobby createdLobby = await LobbyService.Instance.CreateLobbyAsync("NON", LobbyServiceData.maxLobbySize, createLobbyOptions);
                lobby = createdLobby;
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
                throw new Exception("Target lobby ID is null or empty");
            }

            if (lobby != null)
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
                lobby = joinedLobby;

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
            if (lobby == null)
            {
                throw new Exception("All ready leave from lobby");
            }

            if (IsMyLobby())
            {
                await MigrateLobbyHost(lobby.Players[1].Id).StartAndWait();
            }

            await lobbySemaphore.WaitAsync();
            try
            {
                string lobbyId = lobby.Id;
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, AuthenticationServiceManager.Instance.GetUserId());

                Debug.Log($"Leave from lobby {lobbyId}");

                lobby = null;
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
                throw new Exception("You can't kick if you aren't a leader");
            }

            if (string.IsNullOrEmpty(clientID))
            {
                throw new Exception("Kicked player has null ID");
            }

            if (clientID == AuthenticationServiceManager.Instance.GetUserId())
            {
                throw new Exception("You can't kick myself");
            }

            await LobbyService.Instance.RemovePlayerAsync(lobby.Id, clientID);

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
                throw new Exception("You can't migrate host");
            }

            if (string.IsNullOrEmpty(clientID))
            {
                throw new Exception("Target host clien has null ID");
            }

            if (clientID == AuthenticationServiceManager.Instance.GetUserId())
            {
                throw new Exception("You all ready host");
            }

            if (!lobby.Players.Any(p => p.Id == clientID))
            {
                throw new Exception("Target new host id not in lobby");
            }

            await lobbySemaphore.WaitAsync();
            try
            {
                UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
                {
                    HostId = clientID
                };

                Lobby updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateLobbyOptions);
                lobby = updatedLobby;

                Debug.Log("Set new lobby host: " + lobby.HostId);
            }
            finally
            {
                lobbySemaphore.Release();
            }
        });

        return request;
    }

    public Request UpdatePlayerData()
    {
        Request request = new Request(async () => 
        {
            if (lobby == null)
            {
                throw new Exception("You not in lobby");
            }

            UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { LobbyServiceData.PLAYER_NICKNAME_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, HashDataManager.Instance.GetMyNickname()) },
                    { LobbyServiceData.PLAYER_CHARACTER_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, HashDataManager.Instance.GetMyCharacterData().CharacterIndex.ToString()) },
                    { LobbyServiceData.PLAYER_READY_FLAG_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, false.ToString())}
                }
            };

            await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationServiceManager.Instance.GetUserId(), updatePlayerOptions);
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
                    if (lobby != null)
                    {
                        await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
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

                if (lobby == null) { break; }

                await lobbySemaphore.WaitAsync(ct);
                try
                {
                    Lobby updatedLobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

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

                    lobby = updatedLobby;
                    OnLobbyDataUpdate?.Invoke();
                }
                catch (Exception e)
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
            { LobbyServiceData.PLAYER_NICKNAME_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, HashDataManager.Instance.GetMyNickname()) },
            { LobbyServiceData.PLAYER_CHARACTER_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, HashDataManager.Instance.GetMyCharacterData().CharacterIndex.ToString()) },
            { LobbyServiceData.PLAYER_READY_FLAG_FIELD, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, false.ToString())}
        });
    }

    public bool IsLobbyCreated() => lobby != null;

    public string GetLobbyId() => lobby != null ? lobby.Id : string.Empty;

    public bool IsMyLobby() => GetHostID() == AuthenticationServiceManager.Instance.GetUserId();

    public string GetHostID() => lobby != null ? lobby.HostId : string.Empty;

    public List<LobbyUserData> GetLobbyUserList()
    {
        if (lobby == null) { return null; }

        List<LobbyUserData> list = new List<LobbyUserData>();

        foreach (Player player in lobby.Players)
        {
            list.Add(userDataCreator.GetUserData(player));
        }

        return list;
    }

    private void HashDataManager_OnUserDataChanged() => UpdatePlayerData().Start();

    public void Dispose()
    {
        isDisposing = true;

        instance = null;

        loopsCts?.Cancel();

        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnUserDataChanged -= HashDataManager_OnUserDataChanged;
        }
    }
}