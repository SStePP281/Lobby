using System;
using UnityEngine;
using Unity.Services.Friends.Notifications;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class InviteLobbyManager : IDisposable
{
    private static bool isDisposing = false;
    private static InviteLobbyManager instence;
    public static InviteLobbyManager Instance
    {
        get
        {
            if (isDisposing) { return null; }

            if (instence == null)
            {
                instence = new InviteLobbyManager();
            }

            return instence;
        }
    }

    private readonly List<string> inviteRequestList;
    private readonly Dictionary<string, DateTime> myInviteRequestList;

    private InviteLobbyManager()
    {
        inviteRequestList = new List<string>();
        myInviteRequestList = new Dictionary<string, DateTime>();

        FriendServiceManager.Instance.OnMessageReceive += FriendServiceManager_OnMessageReceive;
        FriendServiceManager.Instance.OnFriendListChanged += FriendServiceManager_OnFriendListChanged;
    }

    public void SendLobbyInviteRequest(string targetUserId)
    {
        InviteRequest inviteRequest = new InviteRequest(AuthenticationServiceManager.Instance.GetUserId());

        FriendServiceManager.Instance.SendMessage(targetUserId, inviteRequest, (bool succes, string e) =>
        {
            if (succes)
            {
                myInviteRequestList[targetUserId] = DateTime.UtcNow;

                _ = StartInviteTimeout(targetUserId);

                FriendServiceManager.Instance.TryForceRefresh();
            }
        });
    }

    public void SendLobbyInviteResponse(string targetUserId)
    {
        if (!inviteRequestList.Any(r => r == targetUserId)) { return; }

        InviteResponse inviteResponse = new InviteResponse(AuthenticationServiceManager.Instance.GetUserId());

        FriendServiceManager.Instance.SendMessage(targetUserId, inviteResponse, (bool succes, string e) => 
        { 
            if (succes) 
            { 
                inviteRequestList.Remove(targetUserId);

                FriendServiceManager.Instance.TryForceRefresh();
            } 
        });
    }

    public void SendLobbyInviteCancellation(string targetUserId) // send invite cancellation
    {
        if (!myInviteRequestList.Any(r => r.Key == targetUserId)) { return; }

        InviteCancellation inviteCancellation = new InviteCancellation(AuthenticationServiceManager.Instance.GetUserId());

        FriendServiceManager.Instance.SendMessage(targetUserId, inviteCancellation, (bool succes, string e) =>
        {
            if (succes)
            {
                myInviteRequestList.Remove(targetUserId);

                FriendServiceManager.Instance.TryForceRefresh();

                Debug.Log($"Invite to {targetUserId} expired and removed.");
            }
        });
    }

    private async Task StartInviteTimeout(string targetUserId) // delete old request
    {
        await Task.Delay(TimeSpan.FromSeconds(FriendServiceData.REQUEST_LIFE_TIME));

        if (myInviteRequestList.TryGetValue(targetUserId, out var time))
        {
            if (DateTime.UtcNow - time >= TimeSpan.FromSeconds(FriendServiceData.REQUEST_LIFE_TIME))
            {
                SendLobbyInviteCancellation(targetUserId);
            }
        }
    }

    private void OnLobbyInvityRequest(InviteRequest inviteRequest) // Show info
    {
        if (!inviteRequestList.Any(r => r == inviteRequest.SenderId))
        {
            inviteRequestList.Add(inviteRequest.SenderId);

            FriendServiceManager.Instance.TryForceRefresh();

            Debug.Log($"Receive new request invite message from: {inviteRequest.SenderId}");
        }
    }

    private void OnLobbyInvityRespone(InviteResponse inviteResponse) // Create/take lobby id and send
    {
        Debug.Log($"Receive new respone invite message from: {inviteResponse.SenderId}");

        if (LobbyServiceManager.Instance.IsLobbyCreated())
        {
            LobbyIdMessage message = new LobbyIdMessage(LobbyServiceManager.Instance.GetLobbyId());

            FriendServiceManager.Instance.SendMessage(inviteResponse.SenderId, message, (bool succes, string e) => 
            { 
                if (succes)
                { 
                    myInviteRequestList.Remove(inviteResponse.SenderId);

                    FriendServiceManager.Instance.TryForceRefresh();
                }
            });

            Debug.Log("Send lobby id to invity");
        }
        else
        {
            LobbyServiceManager.Instance.CreateLobby((bool succes, string e) => 
            { 
                if (succes) 
                { 
                    OnLobbyInvityRespone(inviteResponse); 
                } 
            });

            Debug.Log("Crate lobby to invity");
        }
    }

    private void OnLobbyInviteCancellation(InviteCancellation inviteCancellation)
    {
        if (inviteRequestList.Any(r => r == inviteCancellation.SenderId))
        {
            inviteRequestList.Remove(inviteCancellation.SenderId);

            FriendServiceManager.Instance.TryForceRefresh();
        }
    }

    private void OnLobbyIdMessage(LobbyIdMessage lobbyIdMessage) // Connect to lobby
    {
        LobbyServiceManager.Instance.JoinLobbyByID(lobbyIdMessage.LobbyId);

        Debug.Log($"Receive new join lobby id message, lobby id: {lobbyIdMessage.LobbyId}");
    }

    private void FriendServiceManager_OnMessageReceive(IMessageReceivedEvent message)
    {
        BaseMessage baseMessage = message.GetAs<BaseMessage>();

        if (baseMessage == null || string.IsNullOrEmpty(baseMessage.Type)) { return; }

        switch (baseMessage.Type)
        {
            case nameof(InviteRequest):
                InviteRequest inviteRequest = message.GetAs<InviteRequest>();
                if (inviteRequest != null)
                {
                    OnLobbyInvityRequest(inviteRequest);
                }
                break;
            case nameof(InviteResponse):
                InviteResponse inviteResponse = message.GetAs<InviteResponse>();
                if (inviteResponse != null)
                {
                    OnLobbyInvityRespone(inviteResponse);
                }
                break;
            case nameof(InviteCancellation):
                InviteCancellation inviteCancellation = message.GetAs<InviteCancellation>();
                if (inviteCancellation != null)
                {
                    OnLobbyInviteCancellation(inviteCancellation);
                }
                break;
            case nameof(LobbyIdMessage):
                LobbyIdMessage lobbyIdMessage = message.GetAs<LobbyIdMessage>();
                if (lobbyIdMessage != null)
                {
                    OnLobbyIdMessage(lobbyIdMessage);
                }
                break;
            default:
                return;
        }
    }

    private void FriendServiceManager_OnFriendListChanged()
    {
        var validFriends = FriendServiceManager.Instance.GetFriendList().Select(f => f.FriendID).ToHashSet();

        var toRemoveMy = myInviteRequestList.Keys.Where(id => !validFriends.Contains(id)).ToList();
        foreach (var id in toRemoveMy)
            myInviteRequestList.Remove(id);

        var toRemoveIncoming = inviteRequestList.Where(id => !validFriends.Contains(id)).ToList();
        foreach (var id in toRemoveIncoming)
            inviteRequestList.Remove(id);
    }

    public FriendData.RequestType GetRequstType(string userId)
    {
        if (myInviteRequestList.ContainsKey(userId))
        {
            return FriendData.RequestType.MyInvited;
        }
        else if (inviteRequestList.Contains(userId))
        {
            return FriendData.RequestType.WasInvited;
        }    
        else
        {
            return FriendData.RequestType.None;
        }
    }

    public bool CanBeInvited(string targetUserID)
    {
        FriendData friendData = FriendServiceManager.Instance.GetFriendList().FirstOrDefault(f => f.FriendID == targetUserID);

        return friendData.IsOnline && friendData.Type != FriendData.RequestType.MyInvited;
    }

    public bool CanAcceptInvite(string targetUserID)
    {
        return GetRequstType(targetUserID) == FriendData.RequestType.WasInvited;
    }

    public void Dispose()
    {
        instence = null;

        isDisposing = true;

        if (FriendServiceManager.Instance != null)
        {
            FriendServiceManager.Instance.OnMessageReceive -= FriendServiceManager_OnMessageReceive;
            FriendServiceManager.Instance.OnFriendListChanged -= FriendServiceManager_OnFriendListChanged;
        }
    }
}