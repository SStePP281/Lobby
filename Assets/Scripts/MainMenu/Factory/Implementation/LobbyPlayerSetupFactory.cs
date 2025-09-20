using UnityEngine;
using Zenject;

internal class LobbyPlayerSetupFactory : ILobbyPlayerSetupFactory
{
    [Inject] private LobbyPlayerSetup lobbyPlayerSetupPrefab;

    public LobbyPlayerSetup CreatePlayerSetup(Transform container = null)
    {
        if (lobbyPlayerSetupPrefab == null) { return null; }

        return Object.Instantiate(lobbyPlayerSetupPrefab, container);
    }
}