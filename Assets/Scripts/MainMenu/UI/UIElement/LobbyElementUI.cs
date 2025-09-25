using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyElementUI : BaseUIListElement<LobbyUserData>
{
    [Header("Reference")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Transform hostImage;
    [SerializeField] private Transform myImage;

    public override BaseUIListElement<LobbyUserData> Initialize(LobbyUserData userData)
    {
        base.Initialize(userData);

        nicknameText.text = userData.Nickname;
        avatarImage.sprite = userData.Avatar;

        hostImage.gameObject.SetActive(userData.IsHost);
        myImage.gameObject.SetActive(userData.IsMyData);

        return this;
    }
}
