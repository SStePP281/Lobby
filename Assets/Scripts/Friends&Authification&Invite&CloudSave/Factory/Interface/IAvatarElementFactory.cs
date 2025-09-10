using UnityEngine;

public interface IAvatarElementFactory
{
    public AvatarElementUI CreateAvatarElement(Transform container = null);
}
