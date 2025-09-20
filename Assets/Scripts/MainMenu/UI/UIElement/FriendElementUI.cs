using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendElementUI : MonoBehaviour, IPointerClickHandler
{
    public Action<Vector2, string> OnClick;

    [SerializeField] private TextMeshProUGUI friendNameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Outline outline;
    [SerializeField] private Color onlineColor;
    [SerializeField] private Color myInvityColor;
    [SerializeField] private Color invitedColor;

    private string friendID;

    public FriendElementUI Initaliaze(FriendData friendData)
    {
        friendID = friendData.FriendID;

        friendNameText.text = friendData.FriendName.Split("#")[0];
        avatarImage.sprite = friendData.Avatar;

        outline.enabled = true;
        if (friendData.Type == FriendData.RequestType.WasInvited)
        {
            outline.effectColor = invitedColor;
        }
        else if (friendData.Type == FriendData.RequestType.MyInvited)
        {
            outline.effectColor = myInvityColor;
        }
        else if (friendData.IsOnline)
        {
            outline.effectColor = onlineColor;   
        }
        else
        {
            outline.enabled = false;
        }

        return this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnClick?.Invoke(transform.position, friendID);
        }
    }
}
