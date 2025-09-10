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
        if (FriendServiceManager.Instance.TryAddFriend(friendIDField.text, FriendServiceManager_TryAddFriend_Callback))
        {
            InformationUI.Instance.Show("Wait...", false);
        }
    }

    private void FriendServiceManager_TryAddFriend_Callback(bool succes, string errorMessage)
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
}
