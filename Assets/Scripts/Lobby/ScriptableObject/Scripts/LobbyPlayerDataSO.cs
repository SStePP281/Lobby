using UnityEngine;

[CreateAssetMenu]
public class LobbyPlayerDataSO : ScriptableObject
{
    public string characterName;
    public Sprite characterSprite;
    public Transform characterPrefab;
}
