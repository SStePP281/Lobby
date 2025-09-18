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
            if (friendActivity == null)
            {
                friendActivity = HashDataManager.Instance.GetFriendActivity(relationship.Member.Id);
            }

            isOnline = presence.Availability == Availability.Online;
        }

        if (friendActivity == null)
        {
            friendActivity = HashDataManager.Instance.GetFriendActivity(relationship.Member.Id);
        }

        if (friendActivity != null)
        {
            avatarSprite = avatarListSO.GetSpriteByIndex(friendActivity.AvatarIndex);
        }

        return new FriendData
        {
            FriendID = member.Id,
            FriendName = member.Profile?.Name ?? "Unknown",
            Avatar = avatarSprite,
            IsOnline = isOnline,
            Type = InviteLobbyManager.Instance.GetRequstType(member.Id),
        };
    }
}