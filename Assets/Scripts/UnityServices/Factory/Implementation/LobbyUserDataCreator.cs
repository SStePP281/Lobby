using Unity.Services.Lobbies.Models;
using Zenject;

public class LobbyUserDataCreator : ILobbyUserDataCreator
{
    [Inject] private LobbyAvatarListSO avatarListSO;

    public LobbyUserData GetUserData(Player player)
    {
        if (player == null) { return default; }

        bool isMyData = AuthenticationServiceManager.Instance.GetUserId() == player.Id;

        string nickname = string.Empty;
        if (isMyData)
        {
            nickname = "YOU";
        }
        else
        {
            if (player.Data.ContainsKey(LobbyServiceData.PLAYER_NICKNAME_FIELD)) { nickname = player.Data[LobbyServiceData.PLAYER_NICKNAME_FIELD].Value; }
        }

        int characterIndex = 0;
        if (player.Data.ContainsKey(LobbyServiceData.PLAYER_CHARACTER_FIELD)) { characterIndex = int.Parse(player.Data[LobbyServiceData.PLAYER_CHARACTER_FIELD].Value); }

        bool readyFlag = false;
        if (player.Data.ContainsKey(LobbyServiceData.PLAYER_READY_FLAG_FIELD)) { readyFlag = bool.Parse(player.Data[LobbyServiceData.PLAYER_READY_FLAG_FIELD].Value); }

        int avatarIndex = 0;
        if (isMyData)
        {
            avatarIndex = HashDataManager.Instance.GetMyFriendActivity().AvatarIndex;
        }
        else
        {
            avatarIndex = HashDataManager.Instance.GetFriendActivity(player.Id).AvatarIndex;
        }

        return new LobbyUserData
        {
            UserID = player.Id,
            Nickname = nickname.Split("#")[0],
            IsHost = LobbyServiceManager.Instance.GetHostID() == player.Id,
            IsMyData = isMyData,
            Avatar = avatarListSO.GetSpriteByIndex(avatarIndex),
            CharacterIndex = characterIndex,
            IsReady = readyFlag,
        };
    }
}