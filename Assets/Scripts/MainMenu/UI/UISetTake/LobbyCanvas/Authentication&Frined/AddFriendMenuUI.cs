using ServiceRequest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddFriendMenuUI : SystemWindowTrigger
{
    [Header("Reference")]
    [SerializeField] private TMP_InputField friendIDField;
    [SerializeField] private Button addButton;

    protected override void Awake()
    {
        base.Awake();

        addButton.onClick.AddListener(AddFriendButton_OnClick);
    }

    private void AddFriendButton_OnClick() => AddFriend();

    private void AddFriend()
    {
        InformationUI.Instance.Show("Wait...", false);

        FriendServiceManager.Instance.TryAddFriend(friendIDField.text)
            .OnSuccessComplete(() => { InformationUI.Instance.Hide(); window.Close(); })
            .OnFailureComplete(m => InformationUI.Instance.Show(m))
            .Start();
    }
}
