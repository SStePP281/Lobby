using ServiceRequest;
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
    public event Action<string> OnReceiveFriendRequest;

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
        AuthenticationServiceManager.Instance.OnSignIn += AuthenticationServiceManager_OnSignIn;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;

        OnInitialize += FriendServiceManager_OnInitialize;

        _ = InitializeAsync();
    }

    public void SetFriendDataCreator(IFriendDataCreator creator) => friendDataCreator = creator;

    private async Task InitializeAsync()
    {
        if (!UnityServicesManager.Instance.IsInitialized())
        {
            UnityServicesManager.Instance.OnInitialize += UnityServicesManager_OnInitialize;
            return;
        }

        if (!AuthenticationServiceManager.Instance.IsSignedIn) { return; }

        try
        {
            await FriendsService.Instance.InitializeAsync();

            isInit = true;
            Debug.Log("Initialize friend service");

            await FriendsService.Instance.SetPresenceAvailabilityAsync(Availability.Online);

            OnInitialize?.Invoke();
        }
        catch (FriendsServiceException e)
        {
            Debug.Log(e);
        }
    }

    public Request TryAddFriend(string userId)
    {
        Request request = new Request(async () =>
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new Exception("Friend ID cannot be empty.");
            }

            if (FriendsService.Instance.Friends.Any(r => r.Member.Id == userId))
            {
                throw new Exception("This user is already your friend.");
            }

            if (FriendsService.Instance.OutgoingFriendRequests.Any(r => r.Member.Id == userId))
            {
                throw new Exception("Friend request to this user has already been sent.");
            }

            if (FriendsService.Instance.Friends.Count >= FriendServiceData.MAX_FRIEND_COUNT)
            {
                throw new Exception($"You have reached the maximum number of friends ({FriendServiceData.MAX_FRIEND_COUNT}).");
            }

            if (!FriendsService.Instance.IncomingFriendRequests.Any(r => r.Id == userId))
            {
                if (FriendsService.Instance.OutgoingFriendRequests.Count + FriendsService.Instance.IncomingFriendRequests.Count >= FriendServiceData.MAX_REQUEST_COUNT)
                {
                    if (FriendsService.Instance.OutgoingFriendRequests.Count != 0)
                    {
                        await FriendsService.Instance.DeleteOutgoingFriendRequestAsync(FriendsService.Instance.OutgoingFriendRequests[0].Id);
                    }
                    else
                    {
                        throw new Exception("You have reached the maximum number of friend requests. Please cancel an existing request before sending a new one.");
                    }
                }
            }

            await FriendsService.Instance.AddFriendAsync(userId);

            await ForceRelationshipsRefreshAsync();
        });

        return request;
    }

    public Request TryDeleteFriend(string userId)
    {
        Request request = new Request(async () =>
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Friend ID is invalid or missing.");
            }

            await FriendsService.Instance.DeleteFriendAsync(userId);

            await ForceRelationshipsRefreshAsync();
        });

        return request;
    }

    public Request TryDeleteIncomingRequest(string userId)
    {
        Request request = new Request(async () =>
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User ID is invalid or missing.");
            }

            await FriendsService.Instance.DeleteIncomingFriendRequestAsync(userId);

            await ForceRelationshipsRefreshAsync();
        });

        return request;
    }

    public Request TrySetPresenceActivity<T>(T presenceActivity) where T : new()
    {
        Request request = new Request(async () =>
        {
            await FriendsService.Instance.SetPresenceActivityAsync(presenceActivity);

            await ForceRelationshipsRefreshAsync();
        })
        .OnSuccessComplete(() => OnUserDataUpdate?.Invoke());

        return request;
    }

    public Request SendMessage<T>(string targetUserId, T message) where T : BaseMessage, new()
    {
        Request request = new Request(async () =>
        {
            if (string.IsNullOrEmpty(targetUserId))
            {
                throw new Exception("Recipient user ID is invalid or missing.");
            }

            await FriendsService.Instance.MessageAsync(targetUserId, message);

            await ForceRelationshipsRefreshAsync();
        });

        return request;
    }

    public FriendData GetFriendData(string userId)
    {
        if (!isInit) { return default; }

        Relationship relationship = null;

        relationship = FriendsService.Instance.Friends.FirstOrDefault(r => r.Member.Id == userId);
        if (relationship == null)
        {
            relationship = FriendsService.Instance.IncomingFriendRequests.FirstOrDefault(r => r.Member.Id == userId);
        }

        return friendDataCreator.GetFriendData(relationship);
    }

    public List<FriendData> GetFriendList()
    {
        List<FriendData> friendList = new List<FriendData>();

        if (!isInit) { return friendList; }

        foreach (Relationship relationship in FriendsService.Instance.Friends)
        {
            friendList.Add(friendDataCreator.GetFriendData(relationship));
        }

        return friendList;
    }

    public List<FriendData> GetIncomingRequestsList()
    {
        List<FriendData> friendList = new List<FriendData>();

        if (!isInit) { return friendList; }

        foreach (Relationship relationship in FriendsService.Instance.IncomingFriendRequests)
        {
            friendList.Add(friendDataCreator.GetFriendData(relationship));
        }

        return friendList;
    }

    public FriendData GetMyFriendData()
    {
        if (!isInit) { return default; }

        return friendDataCreator.GetMyFriendData();
    }

    public bool IsInitialize() => isInit;

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

    private void FriendsService_PresenceUpdated(IPresenceUpdatedEvent obj) => OnFriendListChanged?.Invoke();

    private void FriendsService_RelationshipAdded(IRelationshipAddedEvent obj)
    {
        if (obj.Relationship.Type == RelationshipType.FriendRequest && obj.Relationship.Member.Role == MemberRole.Source)
        {
            OnReceiveFriendRequest?.Invoke(obj.Relationship.Member.Id);
        }

        OnFriendListChanged?.Invoke();
    }

    private void FriendsService_RelationshipDeleted(IRelationshipDeletedEvent obj) => OnFriendListChanged?.Invoke();

    private void FriendsService_MessageReceived(IMessageReceivedEvent obj) => OnMessageReceive?.Invoke(obj);

    private void FriendServiceManager_OnInitialize()
    {
        FriendsService.Instance.PresenceUpdated += FriendsService_PresenceUpdated;
        FriendsService.Instance.RelationshipAdded += FriendsService_RelationshipAdded;
        FriendsService.Instance.RelationshipDeleted += FriendsService_RelationshipDeleted;
        FriendsService.Instance.MessageReceived += FriendsService_MessageReceived;
    }

    private void AuthenticationServiceManager_OnSignIn()
    {
        if (isInit) { return; }

        _ = InitializeAsync();
    }

    private void AuthenticationServiceManager_OnSignOut()
    {
        isInit = false;

        _ = FriendsService.Instance.SetPresenceAvailabilityAsync(Availability.Offline);
    }

    private void UnityServicesManager_OnInitialize()
    {
        _ = InitializeAsync();

        UnityServicesManager.Instance.OnInitialize -= UnityServicesManager_OnInitialize;
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

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignIn -= AuthenticationServiceManager_OnSignIn;
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }

        instance = null;

        isDisposing = true;
    }
}