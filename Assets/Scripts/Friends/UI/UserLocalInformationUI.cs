using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserLocalInformationUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Image userAvatarImage;
    [SerializeField] private TextMeshProUGUI userNameText;

    private void Awake()
    {
        PlayerDataManager.Instance.OnPlayerDataChanged += PlayerDataManager_OnPlayerDataChanged;
    }

    private void PlayerDataManager_OnPlayerDataChanged() => RefreshUserData();

    private void RefreshUserData()
    {
        string playerName = PlayerDataManager.Instance.GetMyUserName();
        Sprite avatar = PlayerDataManager.Instance.GetMyAvatar();

        userNameText.text = string.IsNullOrEmpty(playerName) ? string.Empty : playerName.Split("#")[0];
        userAvatarImage.sprite = avatar;
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataChanged -= PlayerDataManager_OnPlayerDataChanged;
        }
    }
}
