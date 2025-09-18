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
        HashDataManager.Instance.OnPlayerDataChanged += HashDataManager_OnPlayerDataChanged;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
    }

    private void RefreshUserData()
    {
        string playerName = HashDataManager.Instance.GetMyUserName();
        userNameText.text = string.IsNullOrEmpty(playerName) ? string.Empty : playerName.Split("#")[0];

        FriendActivity friendActivity = HashDataManager.Instance.GetMyFriendActivity();
        if (friendActivity != null )
        {
            userAvatarImage.sprite = avatarListSO.GetSpriteByIndex(friendActivity.AvatarIndex);
        }
    }

    private void AuthenticationServiceManager_OnSignOut()
    {
        userAvatarImage.sprite = null;
        userNameText.text = string.Empty;
    }

    private void HashDataManager_OnPlayerDataChanged() => RefreshUserData();

    private void OnDestroy()
    {
        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnPlayerDataChanged -= HashDataManager_OnPlayerDataChanged;
        }

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }
    }
}
