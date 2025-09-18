using UnityEngine;
using Zenject;

public class ServicesBootstrap : MonoBehaviour
{
    [Inject] private IFriendDataCreator friendDataCreator;
    [Inject] private LobbyAvatarListSO avatarListSO;

    private void Start()
    {
        FriendServiceManager.Instance.SetFriendDataCreator(friendDataCreator);
        HashDataManager.Instance.SetFriendAvatarListSO(avatarListSO);
    }

    private void OnDestroy()
    {
        InviteLobbyManager.Instance.Dispose();
        HashDataManager.Instance.Dispose();
        FriendServiceManager.Instance.Dispose();
        AuthenticationServiceManager.Instance.Dispose();
        UnityServicesManager.Instance.Dispose();
    }
}
