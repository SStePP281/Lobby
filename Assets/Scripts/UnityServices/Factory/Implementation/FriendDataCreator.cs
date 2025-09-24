using Unity.Services.Friends.Models;
using UnityEngine;
using Zenject;

public class FriendDataCreator : IFriendDataCreator
{
    [Inject] private LobbyAvatarListSO avatarListSO;

    public FriendData GetFriendPlayerData(Relationship relationship)
    {
        var member = relationship.Member;
        var presence = member.Presence;
        Sprite avatarSprite = null;
        bool isOnline = false;

        FriendActivity friendActivity = null;
        if (presence != null)
        {
            friendActivity = presence.GetActivity<FriendActivity>();

            isOnline = presence.Availability == Availability.Online;
        }

        if (friendActivity == null || friendActivity.AvatarIndex == -1)
        {
            friendActivity = HashDataManager.Instance.GetFriendActivity(relationship.Member.Id);
        }

        if (friendActivity != null)
        {
            avatarSprite = avatarListSO.GetSpriteByIndex(friendActivity.AvatarIndex);
        }

        return new FriendData
        {
            UserID = member.Id,
            FriendName = member.Profile?.Name.Split("#")[0] ?? "Unknown",
            Avatar = avatarSprite,
            IsOnline = isOnline,
            InLobby = LobbyServiceManager.Instance.LobbyContainsUser(member.Id),
            Type = InviteLobbyManager.Instance.GetRequstType(member.Id),
        };
    }

    public FriendData GetMyFriendData()
    {
        return new FriendData
        {
            UserID = AuthenticationServiceManager.Instance.GetUserId(),
            FriendName = AuthenticationServiceManager.Instance.GetPlayerName(),
            Avatar = avatarListSO.GetSpriteByIndex(HashDataManager.Instance.GetMyFriendActivity().AvatarIndex),
            IsOnline = true,
            InLobby = LobbyServiceManager.Instance.IsLobbyCreated(),
            Type = FriendData.RequestType.None
        };
    }
}