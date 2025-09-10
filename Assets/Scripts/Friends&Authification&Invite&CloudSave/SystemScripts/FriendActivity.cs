using System;

[Serializable]
public class FriendActivity
{
    public FriendActivity() : this (-1) { }

    public FriendActivity(int avatarIndex)
    {
        AvatarIndex = avatarIndex;
    }

    public int AvatarIndex { get; set; }
}