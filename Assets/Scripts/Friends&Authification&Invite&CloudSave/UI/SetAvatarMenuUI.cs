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
            int index = avatarList.GetSpriteIndexBySprite(sprite);

            avatarElementFactory.CreateAvatarElement(avatarContainer).Initialize(sprite, index, AvatarElementUI_OnClick);
        }
    }

    private void SetButton_OnClick() => SetAvatar();

    private void SetAvatar()
    {
        if (selectedAvatar == null) { return; }

        PlayerDataManager.Instance.SetNewAvatar(selectedAvatar, PlayerDataManager_TrySetPresenceActivity_Callback);

        InformationUI.Instance.Show("Wait...", false);
    }

    private void PlayerDataManager_TrySetPresenceActivity_Callback(bool succes, string errorMessage)
    {
        if (succes)
        {
            InformationUI.Instance.Hide();

            window.Close();
        }
        else
        {
            InformationUI.Instance.Show(errorMessage);
        }
    }

    private void AvatarElementUI_OnClick(int selectedIndex)
    {
        selectedAvatar = avatarList.GetAvatarSpriteByIndex(selectedIndex);

        demoImage.sprite = selectedAvatar;
    }

    private void OnDisable()
    {
        selectedAvatar = null;
    }

    private void OnEnable()
    {
        demoImage.sprite = PlayerDataManager.Instance.GetMyAvatar();
    }
}
