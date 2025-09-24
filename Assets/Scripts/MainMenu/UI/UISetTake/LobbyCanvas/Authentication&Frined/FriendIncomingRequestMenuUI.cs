using ServiceRequest;
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
        InformationUI.Instance.Show("Wait...", false);

        FriendServiceManager.Instance.TryAddFriend(selectedPlayerID)
            .OnSuccessComplete(() => InformationUI.Instance.Hide())
            .OnFailureComplete(m => InformationUI.Instance.Show(m))
            .Start();

        HideActionMenu();
    }

    private void DeclineRequest()
    {
        InformationUI.Instance.Show("Wait...", false);

        FriendServiceManager.Instance.TryDeleteIncomingRequest(selectedPlayerID)
            .OnSuccessComplete(() => InformationUI.Instance.Hide())
            .OnFailureComplete(m => InformationUI.Instance.Show(m))
            .Start();

        HideActionMenu();
    }

    private void ShowProfile()
    {
        HideActionMenu();
    }
}
