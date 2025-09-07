using UnityEngine;
using Zenject;

public class FriendElementFactory : IFriendElementFactory
{
    [Inject] private FriendElementUI prefab;

    public FriendElementUI CreateFriendElement(Transform container = null)
    {
        return Object.Instantiate(prefab, container);
    }
}
