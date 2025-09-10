using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Friends;
using Unity.Services.Friends.Exceptions;
using Unity.Services.Friends.Models;
using Unity.Services.Friends.Notifications;
using UnityEngine;

public class FriendServiceManager : IDisposable
{
    public event Action OnInitialize;
    public event Action OnUserDataUpdate;
    public event Action<IMessageReceivedEvent> OnMessageReceive;
    public event Action OnFriendListChanged;

    private static bool isDisposing = false;
    private static FriendServiceManager instance;
    public static FriendServiceManager Instance
    {
        get
        {
            if (isDisposing) { return null; }

            if (instance == null)
            {
                instance = new FriendServiceManager();
            }

            return instance;
        }
    }

    private bool isInit;
    private IFriendDataCreator friendDataCreator;

    private FriendServiceManager()
    {
        OnInitialize += FriendServiceManager_OnInitialize;

        _ = InitializeAsync();
    }

    public void SetFriendDataCreator(IFriendDataCreator creator) => friendDataCreator = creator;

    public async Task InitializeAsync()
    {
        if (!UnityServicesManager.Instance.IsInitialized())
        {
            await UnityServicesManager.Instance.Initialize();

            Debug.Log("Friend service manager initialize unity service manager");
        }

        if (!AuthenticationServiceManager.Instance.IsSignedIn)
        {
            AuthenticationServiceManager.Instance.OnSignIn += AuthenticationServiceManager_OnSignIn;
            return;
        }

        try
        {
            await FriendsService.Instance.InitializeAsync();
            await FriendsService.Instance.SetPresenceAvailabilityAsync(Availability.Online);

            Debug.Log("Initilize frined service");

            OnInitialize?.Invoke();
        }
        catch (FriendsServiceException e)
        {
            Debug.Log(e);
        }
    }

    public bool TryAddFriend(string userId, Action<bool, string> callback = null)
    {
        if (string.IsNullOrWhiteSpace(userId)) return false;

        _ = AddNewFriendAsync(userId, callback);

        return true;
    }

    public bool TryDeleteFriend(string userId, Action<bool, string> callback = null)
    {
        if (string.IsNullOrEmpty(userId)) return false;

        _ = DeleteFriendAsync(userId, callback);

        return true;
    }

    public bool TryDeleteIncomingRequest(string userId, Action<bool, string> callback = null)
    {
        if (string.IsNullOrEmpty(userId)) return false;

        _ = DeleteIncomingFriendRequestAsync(userId, callback);

        return true;
    }

    public bool TryForceRefresh()
    {
        _ = ForceRelationshipsRefreshAsync();

        return true;
    }

    public void TrySetPresenceActivity<T>(T presenceActivity, Action<bool, string> callbac = null) where T : new()
    {
        _ = SetPresenceActivityAsync(presenceActivity, callbac);
    }

    public void SendMessage<T>(string targetUserId, T message, Action<bool, string> callback = null) where T : BaseMessage, new()
    {
        if (string.IsNullOrEmpty(targetUserId)) { return; }

        _ = SendMessageAsync(targetUserId, message, callback);
    }

    private async Task AddNewFriendAsync(string userId, Action<bool, string> callback = null)
    {
        if (FriendsService.Instance.Friends.Count >= FriendServiceData.MAX_FRINED_COUNT)
        {
            callback?.Invoke(false, "The maximum number of friends has been achieved");
            return;
        }

        if (!FriendsService.Instance.IncomingFriendRequests.Any(r => r.Id == userId))
        {
            if (FriendsService.Instance.OutgoingFriendRequests.Count + FriendsService.Instance.IncomingFriendRequests.Count >= FriendServiceData.MAX_REQUEST_COUNT)
            {
                if (FriendsService.Instance.OutgoingFriendRequests.Count != 0)
                {
                    await DeleteOutgoingFriendRequestAsync(FriendsService.Instance.OutgoingFriendRequests[0].Id);
                }
                else
                {
                    callback?.Invoke(false, "The number of queries has been exceeded, delete not relevant");
                    return;
                }
            }
        }

        try
        {
            await FriendsService.Instance.AddFriendAsync(userId);

            Debug.Log($"Send new friend request to: {userId}");

            await ForceRelationshipsRefreshAsync();

            callback?.Invoke(true, string.Empty);
        }
        catch (FriendsServiceException e)
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

    private async Task DeleteFriendAsync(string userId, Action<bool, string> callback = null)
    {
        try
        {
            await FriendsService.Instance.DeleteFriendAsync(userId);

            Debug.Log($"Deleted friend {userId}");

            await ForceRelationshipsRefreshAsync();

            callback?.Invoke(true, string.Empty);
        }
        catch (FriendsServiceException e)
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

    private async Task DeleteIncomingFriendRequestAsync(string userId, Action<bool, string> callback = null)
    {
        try
        {
            await FriendsService.Instance.DeleteIncomingFriendRequestAsync(userId);

            Debug.Log($"Deleted incoming friend request from {userId}");

            await ForceRelationshipsRefreshAsync();

            callback?.Invoke(true, string.Empty);
        }
        catch (FriendsServiceException e)
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

    private async Task DeleteOutgoingFriendRequestAsync(string userId, Action<bool, string> callback = null)
    {
        try
        {
            await FriendsService.Instance.DeleteOutgoingFriendRequestAsync(userId);

            Debug.Log($"Deleted outgoing friend request to {userId}");

            await ForceRelationshipsRefreshAsync();

            callback?.Invoke(true, string.Empty);
        }
        catch (FriendsServiceException e)
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

    private async Task ForceRelationshipsRefreshAsync()
    {
        try
        {
            await FriendsService.Instance.ForceRelationshipsRefreshAsync();

            Debug.Log($"Friend service data refreshed");

            OnFriendListChanged?.Invoke();
        }
        catch (FriendsServiceException e)
        {
            Debug.Log(e);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private async Task SendMessageAsync<T>(string targetUserId, T message, Action<bool, string> callback = null) where T : BaseMessage, new()
    {
        try
        {
            await FriendsService.Instance.MessageAsync(targetUserId, message);

            Debug.Log($"Send message");

            callback?.Invoke(true, string.Empty);
        }
        catch (FriendsServiceException e)
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

    private async Task SetPresenceActivityAsync<T>(T presenceActivity, Action<bool, string> callbac = null) where T : new()
    {
        try
        {
            await FriendsService.Instance.SetPresenceActivityAsync(presenceActivity);

            Debug.Log("Update player PresenceActivity");

            await FriendsService.Instance.ForceRelationshipsRefreshAsync();

            OnUserDataUpdate?.Invoke();

            callbac?.Invoke(true, string.Empty);
        }
        catch (FriendsServiceException e)
        {
            Debug.Log(e);

            callbac?.Invoke(false, e.Message);
        }
    }

    public List<FriendData> GetFriendList()
    {
        List<FriendData> frinedList = new List<FriendData>();

        if (!isInit) { return frinedList; }

        foreach (Relationship relationship in FriendsService.Instance.Friends)
        {
            frinedList.Add(friendDataCreator.GetFriendPlayerData(relationship)); 
        }

        return frinedList;
    }

    public List<FriendData> GetIncomingRequestsList()
    {
        List<FriendData> frinedList = new List<FriendData>();

        if (!isInit) { return frinedList; }

        foreach (Relationship relationship in FriendsService.Instance.IncomingFriendRequests)
        {
            frinedList.Add(friendDataCreator.GetFriendPlayerData(relationship));
        }

        return frinedList;
    }

    private void FriendsService_PresenceUpdated(IPresenceUpdatedEvent obj) => OnFriendListChanged?.Invoke();

    private void FriendsService_RelationshipAdded(IRelationshipAddedEvent obj) => OnFriendListChanged?.Invoke();

    private void FriendsService_RelationshipDeleted(IRelationshipDeletedEvent obj) => OnFriendListChanged?.Invoke();

    private void FriendsService_MessageReceived(IMessageReceivedEvent obj) => OnMessageReceive?.Invoke(obj);

    private void FriendServiceManager_OnInitialize()
    {
        isInit = true;

        FriendsService.Instance.PresenceUpdated += FriendsService_PresenceUpdated;
        FriendsService.Instance.RelationshipAdded += FriendsService_RelationshipAdded;
        FriendsService.Instance.RelationshipDeleted += FriendsService_RelationshipDeleted;
        FriendsService.Instance.MessageReceived += FriendsService_MessageReceived;
    }

    private void AuthenticationServiceManager_OnSignIn()
    {
        _ = InitializeAsync();

        AuthenticationServiceManager.Instance.OnSignIn -= AuthenticationServiceManager_OnSignIn;
    }

    public void Dispose()
    {
        OnInitialize -= FriendServiceManager_OnInitialize;

        if (FriendsService.Instance != null)
        {
            FriendsService.Instance.PresenceUpdated -= FriendsService_PresenceUpdated;
            FriendsService.Instance.RelationshipAdded -= FriendsService_RelationshipAdded;
            FriendsService.Instance.RelationshipDeleted -= FriendsService_RelationshipDeleted;
            FriendsService.Instance.MessageReceived -= FriendsService_MessageReceived;
        }

        instance = null;

        isDisposing = true;
    }
}
