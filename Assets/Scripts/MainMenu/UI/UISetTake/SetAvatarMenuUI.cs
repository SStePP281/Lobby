using ServiceRequest;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SetAvatarMenuUI : SystemWindowTrigger
{
    [Inject] private FriendAvatarListSO avatarList;
    [Inject] private IAvatarElementFactory avatarElementFactory;

    [Header("Reference")]
    [SerializeField] private Transform avatarContainer;
    [SerializeField] private Image demoImage;
    [SerializeField] private Button setButton;

    private Sprite selectedAvatar;

    protected override void Awake()
    {
        base.Awake();

        setButton.onClick.AddListener(SetButton_OnClick);
    }

    private void Start()
    {
        foreach (Sprite sprite in avatarList.AvatarList)
        {
            int index = avatarList.GetIndexBySprite(sprite);

            avatarElementFactory.CreateAvatarElement(avatarContainer).Initialize(sprite, index, AvatarElementUI_OnClick);
        }
    }

    private void SetButton_OnClick() => SetAvatar();

    private void SetAvatar()
    {
        if (selectedAvatar == null) { return; }

        FriendActivity activity = HashDataManager.Instance.GetMyFriendActivity();
        activity.AvatarIndex = avatarList.GetIndexBySprite(selectedAvatar);

        InformationUI.Instance.Show("Wait...", false);

        HashDataManager.Instance.SetMyFriendActivity(activity)
            .OnSuccessComplete(() => { InformationUI.Instance.Hide(); window.Close(); })
            .OnFailureComplete(m => InformationUI.Instance.Show(m))
            .Start();
    }

    private void AvatarElementUI_OnClick(int selectedIndex)
    {
        selectedAvatar = avatarList.GetSpriteByIndex(selectedIndex);

        demoImage.sprite = selectedAvatar;
    }

    private void OnDisable()
    {
        selectedAvatar = null;
    }

    private void OnEnable()
    {
        FriendActivity friendActivity = HashDataManager.Instance.GetMyFriendActivity();

        if (friendActivity != null)
        {
            demoImage.sprite = avatarList.GetSpriteByIndex(friendActivity.AvatarIndex);
        }
    }
}
