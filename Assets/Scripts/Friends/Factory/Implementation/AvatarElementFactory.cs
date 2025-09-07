using UnityEngine;
using Zenject;

public class AvatarElementFactory : IAvatarElementFactory
{
    [Inject] private AvatarElementUI prefab;

    public AvatarElementUI CreateAvatarElement(Transform container = null)
    {
        if (prefab == null) { return null; }

        return Object.Instantiate(prefab, container);
    }
}