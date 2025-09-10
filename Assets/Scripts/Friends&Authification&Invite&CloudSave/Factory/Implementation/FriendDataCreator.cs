using Unity.Services.Friends.Models;
using UnityEngine;
using Zenject;

public class FriendDataCreator : IFriendDataCreator
{
    [Inject] private FriendAvatarListSO avatarListSO;

    public FriendData GetFriendPlayerData(Relationship relationship)
    {
        var member = relationship.Member;
        var presence = member.Presence;
        Sprite avatarSprite = null;
        bool isOnline = false;

        if (presence != null)
        {
            FriendActivity friendActivity = presence.GetActivity<FriendActivity>();
            if (friendActivity == null)
            {
                friendActivity = PlayerDataManager.Instance.GetFriendActivity(relationship.Member.Id);
            }

            if (friendActivity != null)
            {
                avatarSprite = avatarListSO.GetSpriteByIndex(friendActivity.AvatarIndex);

                if (avatarSprite == null)
                {
                    avatarSprite = avatarListSO.GetSpriteByIndex(PlayerDataManager.Instance.GetFriendActivity(relationship.Member.Id).AvatarIndex);
                }
            }

            isOnline = presence.Availability == Availability.Online;
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