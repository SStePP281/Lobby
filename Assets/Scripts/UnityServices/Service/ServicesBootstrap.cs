using UnityEngine;
using Zenject;

public class ServicesBootstrap : MonoBehaviour
{
    [Inject] private IFriendDataCreator friendDataCreator;
    [Inject] private ILobbyUserDataCreator lobbyUserDataCreator;
    [Inject] private LobbyAvatarListSO avatarListSO;

    private void Start()
    {
        FriendServiceManager.Instance.SetFriendDataCreator(friendDataCreator);
        HashDataManager.Instance.SetFriendAvatarListSO(avatarListSO);
        LobbyServiceManager.Instance.SetLobbyUserDataCreator(lobbyUserDataCreator);
    }

    private async void OnDestroy()
    {
        if (LobbyServiceManager.Instance.IsLobbyCreated())
        {
            await LobbyServiceManager.Instance.LeaveFromLobby().StartAndWait();
        }

        LobbyServiceManager.Instance.Dispose();
        InviteLobbyManager.Instance.Dispose();
        HashDataManager.Instance.Dispose();
        FriendServiceManager.Instance.Dispose();
        AuthenticationServiceManager.Instance.Dispose();
        UnityServicesManager.Instance.Dispose();
    }
}
