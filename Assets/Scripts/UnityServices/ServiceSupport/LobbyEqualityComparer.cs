using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

public class LobbyEqualityComparer : IEqualityComparer<Lobby>
{
    public bool Equals(Lobby x, Lobby y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.Id == y.Id && x.Players.Count == y.Players.Count && x.HostId == y.HostId;
    }

    public int GetHashCode(Lobby l)
    {
        if (l == null) return 0;

        return l.Id.GetHashCode();
    }
}
