using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

public class LobbyEqualsComparer : IEqualityComparer<Lobby>
{
    public bool Equals(Lobby x, Lobby y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.Id == y.Id && x.LobbyCode == y.LobbyCode && x.HostId == y.HostId;
    }

    public int GetHashCode(Lobby x)
    {
        return x == null ? 0 : x.Id.GetHashCode();
    }
}