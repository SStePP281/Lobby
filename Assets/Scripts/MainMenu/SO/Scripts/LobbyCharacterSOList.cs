using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LobbyCharacterSOList", menuName = "Game/Lobby/Character/LobbyCharacterSOList")]
public class LobbyCharacterSOList : ScriptableObject
{
    public List<LobbyCharacterSO> List;
}
