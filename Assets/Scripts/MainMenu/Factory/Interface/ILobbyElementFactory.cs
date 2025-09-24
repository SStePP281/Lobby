using UnityEngine;

public interface ILobbyElementFactory
{
    public LobbyElementUI CreateElement(Transform container = null);
}