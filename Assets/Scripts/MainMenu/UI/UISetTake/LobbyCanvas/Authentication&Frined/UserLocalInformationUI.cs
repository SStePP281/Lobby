using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UserLocalInformationUI : MonoBehaviour
{
    [Inject] private LobbyAvatarListSO avatarListSO;

    [Header("Reference")]
    [SerializeField] private Image userAvatarImage;
    [SerializeField] private TextMeshProUGUI userNameText;

    private void Awake()
    {
        HashDataManager.Instance.OnUserDataChanged += HashDataManager_OnUserDataChanged;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
    }

    private void RefreshUserData()
    {
        userNameText.text = HashDataManager.Instance.GetMyNickname();
        userAvatarImage.sprite = avatarListSO.GetSpriteByIndex(HashDataManager.Instance.GetMyFriendActivity().AvatarIndex);
    }

    private void AuthenticationServiceManager_OnSignOut()
    {
        userAvatarImage.sprite = null;
        userNameText.text = string.Empty;
    }

    private void HashDataManager_OnUserDataChanged() => RefreshUserData();

    private void OnDestroy()
    {
        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnFriendDataChanged -= HashDataManager_OnUserDataChanged;
        }

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }
    }
}
