using UnityEngine;

public interface IFriendElementFactory
{
    public FriendElementUI CreateFriendElement(Transform container = null);
}
