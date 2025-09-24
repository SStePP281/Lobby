using UnityEngine;
using Zenject;

public class LobbyElementFactory : ILobbyElementFactory
{
    [Inject] private LobbyElementUI lobbyElementUIPrefab;

    public LobbyElementUI CreateElement(Transform container = null)
    {
        if (lobbyElementUIPrefab == null) { return null; }

        return Object.Instantiate(lobbyElementUIPrefab, container);
    }
}