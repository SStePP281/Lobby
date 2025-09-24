using Unity.Services.Friends.Models;
using UnityEngine;

public struct FriendData : IUserData
{
    public enum RequestType
    {
        None,
        MyInvited,
        WasInvited
    }

    public string UserID { get; set; }
    public string FriendName;
    public Sprite Avatar;
    public bool IsOnline;
    public bool InLobby;
    public RequestType Type;
}

public interface IFriendDataCreator
{
    public FriendData GetFriendPlayerData(Relationship relationship);

    public FriendData GetMyFriendData();
}