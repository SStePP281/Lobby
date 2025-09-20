using Unity.Services.Lobbies.Models;

public struct LobbyUserData
{
    public string UserID;
    public string Nickname;
    public int CharacterIndex;
    public bool IsReady;
    public bool IsHost;
}

public interface ILobbyUserDataCreator
{
    public LobbyUserData GetUserData(Player player);
}