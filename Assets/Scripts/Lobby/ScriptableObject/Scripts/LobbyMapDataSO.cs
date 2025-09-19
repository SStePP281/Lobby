using System.Collections.Generic;
using System;
using UnityEngine;

public class LobbyMapDataSOComparer : IEqualityComparer<LobbyMapDataSO>
{
    public bool Equals(LobbyMapDataSO x, LobbyMapDataSO y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.sceneViewName == y.sceneViewName;
    }

    public int GetHashCode(LobbyMapDataSO obj)
    {
        return HashCode.Combine(obj.sceneViewName, obj.sceneView);
    }
}

[CreateAssetMenu]
public class LobbyMapDataSO : ScriptableObject
{
    public string sceneViewName;
    public Sprite sceneView;
}
