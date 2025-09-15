using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarListSO", menuName = "Game/Lobby/FriendData/AvatarListSO")]
public class FriendAvatarListSO : ScriptableObject
{
    public List<Sprite> AvatarList;

    public Sprite GetSpriteByIndex(int index)
    {
        return index >= 0 && index < AvatarList.Count ? AvatarList[index] : null;
    }
    
    public int GetIndexBySprite(Sprite sprite)
    {
        return sprite == null ? 0 : AvatarList.IndexOf(sprite);
    }
}