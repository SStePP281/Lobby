using Unity.Services.Lobbies.Models;
using UnityEngine;

public struct LobbyUserData : IUserData
{
    public string UserID {get; set;}
    public string Nickname;
    public bool IsHost;
    public bool IsMyData;
    public Sprite Avatar;
    public int CharacterIndex;
    public bool IsReady;
}

public interface ILobbyUserDataCreator
{
    public LobbyUserData GetUserData(Player player);
}