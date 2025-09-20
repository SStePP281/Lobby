using Unity.Services.Lobbies.Models;

public class LobbyUserDataCreator : ILobbyUserDataCreator
{
    public LobbyUserData GetUserData(Player player)
    {
        if (player == null) { return default; }

        string nickname = string.Empty;
        if (player.Data.ContainsKey(LobbyServiceData.PLAYER_NICKNAME_FIELD)) { nickname = player.Data[LobbyServiceData.PLAYER_NICKNAME_FIELD].Value; }

        int characterIndex = 0;
        if (player.Data.ContainsKey(LobbyServiceData.PLAYER_CHARACTER_FIELD)) { characterIndex = int.Parse(player.Data[LobbyServiceData.PLAYER_CHARACTER_FIELD].Value); }

        bool readyFlag = false;
        if (player.Data.ContainsKey(LobbyServiceData.PLAYER_READY_FLAG_FIELD)) { readyFlag = bool.Parse(player.Data[LobbyServiceData.PLAYER_READY_FLAG_FIELD].Value); }

        return new LobbyUserData
        {
            UserID = player.Id,
            Nickname = nickname,
            CharacterIndex = characterIndex,
            IsReady = readyFlag,
            IsHost = LobbyServiceManager.Instance.GetHostID() == player.Id
        };
    }
}