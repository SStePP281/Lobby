using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UserLocalInformationUI : MonoBehaviour
{
    [Inject] private FriendAvatarListSO avatarListSO;

    [Header("Reference")]
    [SerializeField] private Image userAvatarImage;
    [SerializeField] private TextMeshProUGUI userNameText;

    private void Awake()
    {
        PlayerDataManager.Instance.OnPlayerDataChanged += PlayerDataManager_OnPlayerDataChanged;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
    }

    private void RefreshUserData()
    {
        string playerName = PlayerDataManager.Instance.GetMyUserName();
        userNameText.text = string.IsNullOrEmpty(playerName) ? string.Empty : playerName.Split("#")[0];

        FriendActivity friendActivity = PlayerDataManager.Instance.GetMyFriendActivity();
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

    private void PlayerDataManager_OnPlayerDataChanged() => RefreshUserData();

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataChanged -= PlayerDataManager_OnPlayerDataChanged;
        }

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }
    }
}
