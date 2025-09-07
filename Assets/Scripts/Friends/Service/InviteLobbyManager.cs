using System;
using UnityEngine;
using Unity.Services.Friends.Notifications;
using System.Collections.Generic;
using System.Linq;

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

    private List<string> inviteRequestMessageList;
    private List<string> myIntiveRequestMessageList;

    private InviteLobbyManager()
    {
        inviteRequestMessageList = new List<string>();
        myIntiveRequestMessageList = new List<string>();

        FriendServiceManager.Instance.OnMessageReceive += FriendServiceManager_OnMessageReceive;
    }

    public void SendLobbyInviteRequest(string targetUserId)
    {
        InviteRequest inviteRequest = new InviteRequest(AuthenticationServiceManager.Instance.GetUserId());

        FriendServiceManager.Instance.SendMessage(targetUserId, inviteRequest, (bool succes, string e) => 
        { 
            if (succes) 
            { 
                myIntiveRequestMessageList.Add(targetUserId); 

                FriendServiceManager.Instance.TryForceRefresh();
            }
        });
    }

    public void SendLobbyInviteResponse(string targetUserId)
    {
        if (!inviteRequestMessageList.Any(r => r == targetUserId)) { return; }

        InviteResponse inviteResponse = new InviteResponse(AuthenticationServiceManager.Instance.GetUserId());

        FriendServiceManager.Instance.SendMessage(targetUserId, inviteResponse, (bool succes, string e) => 
        { 
            if (succes) 
            { 
                inviteRequestMessageList.Remove(targetUserId);

                FriendServiceManager.Instance.TryForceRefresh();
            } 
        });
    }

    private void OnLobbyInvityRequest(InviteRequest inviteRequest) // Show info
    {
        if (!inviteRequestMessageList.Any(r => r == inviteRequest.SenderId))
        {
            inviteRequestMessageList.Add(inviteRequest.SenderId);

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
                    myIntiveRequestMessageList.Remove(inviteResponse.SenderId);

                    FriendServiceManager.Instance.TryForceRefresh();
                }
            });
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

        FriendServiceManager.Instance.TryForceRefresh();
    }

    public FriendData.RequestType GetRequstType(string userId)
    {
        if (myIntiveRequestMessageList.Contains(userId))
        {
            return FriendData.RequestType.MyInvited;
        }
        else if (inviteRequestMessageList.Contains(userId))
        {
            return FriendData.RequestType.WasInvited;
        }    
        else
        {
            return FriendData.RequestType.None;
        }
    }

    public void Dispose()
    {
        instence = null;

        isDisposing = true;

        if (FriendServiceManager.Instance != null)
        {
            FriendServiceManager.Instance.OnMessageReceive += FriendServiceManager_OnMessageReceive;
        }
    }
}