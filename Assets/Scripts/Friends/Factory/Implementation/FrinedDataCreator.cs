using Unity.Services.Friends.Models;
using Zenject;

public class FrinedDataCreator : IFriendDataCreator
{
    [Inject] private FriendAvatarListSO avatarListSO;

    public FriendData GetFriendPlayerData(Relationship relationship)
    {
        var member = relationship.Member;
        int avatarIndex = member.Presence.GetActivity<FriendActivity>().AvatarIndex;

        return new FriendData
        {
            FriendID = member.Id,
            FriendName = member.Profile?.Name ?? "Unknown",
            Avatar = avatarListSO.GetAvatarSpriteByIndex(avatarIndex),
            IsOnline = member.Presence.Availability == Availability.Online,
            Type = InviteLobbyManager.Instance.GetRequstType(member.Id),
        };
    }
}