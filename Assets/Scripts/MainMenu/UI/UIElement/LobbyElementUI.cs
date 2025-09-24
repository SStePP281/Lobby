using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyElementUI : MonoBehaviour, IPointerClickHandler
{
    public event Action<Vector2, string> OnClick;

    [Header("Reference")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image hostImage;
    [SerializeField] private Image myImage;

    private string userID;

    public LobbyElementUI Initialize(LobbyUserData userData)
    {
        userID = userData.UserID;
        nicknameText.text = userData.Nickname;
        avatarImage.sprite = userData.Avatar;

        hostImage.gameObject.SetActive(userData.IsHost);

        myImage.gameObject.SetActive(userData.IsMyData);

        return this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnClick?.Invoke(transform.position, userID);
        }
    }
}
