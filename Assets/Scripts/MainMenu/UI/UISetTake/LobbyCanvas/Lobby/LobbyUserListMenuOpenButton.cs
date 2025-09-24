using UnityEngine;

public class LobbyUserListMenuOpenButton : MonoBehaviour
{
    private void Awake()
    {
        LobbyServiceManager.Instance.OnLobbyCreated += Show;
        LobbyServiceManager.Instance.OnLobbyClosed += Hide;

        Hide();
    }

    private void Show() => gameObject.SetActive(true);

    private void Hide() => gameObject.SetActive(false);

    private void OnDestroy()
    {
        if (LobbyServiceManager.Instance != null)
        {
            LobbyServiceManager.Instance.OnLobbyCreated -= Show;
            LobbyServiceManager.Instance.OnLobbyClosed -= Hide;
        }
    }
}
