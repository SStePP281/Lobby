using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarListSO", menuName = "Game/Lobby/FriendData/AvatarListSO")]
public class FriendAvatarListSO : ScriptableObject
{
    public List<Sprite> AvatarList;

    public Sprite GetAvatarSpriteByIndex(int index)
    {
        return index >= 0 && index < AvatarList.Count ? AvatarList[index] : null;
    }
    
    public int GetSpriteIndexBySprite(Sprite sprite)
    {
        return sprite == null ? 0 : AvatarList.IndexOf(sprite);
    }
}