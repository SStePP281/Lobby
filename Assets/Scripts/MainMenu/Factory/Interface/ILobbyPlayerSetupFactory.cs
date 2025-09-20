using UnityEngine;

public interface ILobbyPlayerSetupFactory
{
    public LobbyPlayerSetup CreatePlayerSetup(Transform container = null);
}