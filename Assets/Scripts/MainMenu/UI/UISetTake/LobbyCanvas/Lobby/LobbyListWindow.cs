public class LobbyListWindow : SystemWindowTrigger
{
    protected override void Awake()
    {
        base.Awake();

        LobbyServiceManager.Instance.OnLobbyClosed += LobbyServiceManager_OnLobbyClosed;
    }

    private void LobbyServiceManager_OnLobbyClosed() => window.Close();

    private void OnDestroy()
    {
        if (LobbyServiceManager.Instance != null)
        {
            LobbyServiceManager.Instance.OnLobbyClosed -= LobbyServiceManager_OnLobbyClosed;
        }
    }
}