using UnityEngine;

public class ServicesBootstrap : MonoBehaviour
{
    [SerializeField] private FriendAvatarListSO avatarListSO;

    private void Start()
    {
        PlayerDataManager.Instance.
    }

    private void OnDestroy()
    {
        FriendServiceManager.Instance.Dispose();
        AuthenticationServiceManager.Instance.Dispose();
        UnityServicesManager.Instance.Dispose();
        PlayerDataManager.Instance.Dispose();
    }
}
