using Unity.Services.Friends.Models;
using UnityEngine;

public struct FriendData
{
    public enum RequestType
    {
        None,
        MyInvited,
        WasInvited
    }

    public string FriendID;
    public string FriendName;
    public Sprite Avatar;
    public bool IsOnline;
    public RequestType Type;
}

public interface IFriendDataCreator
{
    public FriendData GetFriendPlayerData(Relationship relationship);
}