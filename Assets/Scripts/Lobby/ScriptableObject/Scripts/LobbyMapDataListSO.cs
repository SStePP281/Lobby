using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LobbyMapDataListSO : ScriptableObject
{
    public LobbyMapDataSO defaultMap;
    public List<LobbyMapDataSO> mapList;
}
