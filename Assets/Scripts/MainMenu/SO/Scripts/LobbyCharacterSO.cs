using UnityEngine;

[CreateAssetMenu(fileName = "LobbyCharacterSO", menuName = "Game/Lobby/Character/LobbyCharacterSO")]
public class LobbyCharacterSO : ScriptableObject
{
    public string characterName;
    public Sprite characterSprite;
    public Transform characterPrefab;
}
