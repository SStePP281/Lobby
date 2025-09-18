using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LobbyAvatarListSO", menuName = "Game/Lobby/FriendData/LobbyAvatarListSO")]
public class LobbyAvatarListSO : ScriptableObject
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