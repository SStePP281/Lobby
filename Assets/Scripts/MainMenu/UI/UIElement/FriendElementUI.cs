using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendElementUI : BaseUIListElement<FriendData>
{
    [Header("Reference")]
    [SerializeField] private TextMeshProUGUI friendNameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Outline outline;
    [SerializeField] private Color onlineColor;
    [SerializeField] private Color myInvityColor;
    [SerializeField] private Color invitedColor;
    [SerializeField] private Color inLobbyColor;

    public override BaseUIListElement<FriendData> Initialize(FriendData friendData)
    {
        base.Initialize(friendData);

        friendNameText.text = friendData.FriendName;
        avatarImage.sprite = friendData.Avatar;

        outline.enabled = true;
        if (friendData.InLobby)
        {
            outline.effectColor = inLobbyColor;
        }
        else if (friendData.Type == FriendData.RequestType.WasInvited)
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
}
