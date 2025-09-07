using System.Collections.Generic;
using UnityEngine;

public class FriendIncomingRequestMenuUI : FriendListUI
{
    protected override List<FriendData> GetFriendList() => FriendServiceManager.Instance.GetIncomingRequestsList();

    protected override void OnFriendElementClick(Vector2 elementPos, string playerID)
    {
        selectedPlayerID = playerID;

        tabActionGroupUI.Initialize(new List<TabNameAction>
        {
            new TabNameAction("Profile", ShowProfile),
            new TabNameAction("Accept", AcceptRequest),
            new TabNameAction("Decline", DeclineRequest)
        });

        tabActionGroupUI.SetPosition(elementPos);
        tabActionGroupUI.Show();
    }

    private void AcceptRequest()
    {
        if (FriendServiceManager.Instance.TryAddFriend(selectedPlayerID, FriendServiceManager_Callback))
        {
            InformationUI.Instance.Show("Wait...", false);
        }
        else
        {
            InformationUI.Instance.Show("Invalid operation");
        }

        tabActionGroupUI.Hide();
    }

    private void DeclineRequest()
    {
        if (FriendServiceManager.Instance.TryDeleteIncomingRequest(selectedPlayerID, FriendServiceManager_Callback))
        {
            InformationUI.Instance.Show("Wait...", false);
        }
        else
        {
            InformationUI.Instance.Show("Invalid operation");
        }

        tabActionGroupUI.Hide();
    }

    private void ShowProfile()
    {
        tabActionGroupUI.Hide();
    }

    private void FriendServiceManager_Callback(bool succes, string errorMessage)
    {
        if (succes)
        {
            InformationUI.Instance.Hide();
        }
        else
        {
            InformationUI.Instance.Show(errorMessage);
        }
    }
}
