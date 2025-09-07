using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LobbyPlayerDataListSO : ScriptableObject
{
    public LobbyPlayerDataSO defaultCharacter;
    public List<LobbyPlayerDataSO> characterList;
}
