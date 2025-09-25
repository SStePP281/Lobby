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
        FriendServiceManager.Instance.TryAddFriend(friendIDField.text)
            .OnStartRun(() => InformationUI.Instance.Show("Wait...", false))
            .OnSuccessComplete(() => { InformationUI.Instance.Hide(); window.Close(); })
            .OnFailureComplete(m => InformationUI.Instance.Show(m))
            .Start();
    }

    private void OnDisable()
    {
        friendIDField.text = string.Empty;
    }
}
