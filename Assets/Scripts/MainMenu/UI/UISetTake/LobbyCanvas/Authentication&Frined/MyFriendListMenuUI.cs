using ServiceRequest;
using System.Collections.Generic;
using UnityEngine;

public class MyFriendListMenuUI : FriendListUI
{ 
    protected override List<FriendData> GetFriendList() => FriendServiceManager.Instance.GetFriendList();

    protected override void OnFriendElementClick(Vector2 elementPos, string playerID)
    {
        selectedPlayerID = playerID;

        List<TabNameAction> actionList = new List<TabNameAction>
        {
            new TabNameAction("Profile", ShowProfile),
            new TabNameAction("Delete", DeleteFromFriend)
        };

        if (InviteLobbyManager.Instance.CanBeInvited(selectedPlayerID))
        {
            actionList.Insert(1, new TabNameAction("Invite", InviteForLobby));
        }

        if (InviteLobbyManager.Instance.CanAcceptInvite(selectedPlayerID))
        {
            actionList.Insert(1, new TabNameAction("Accept request", AcceptRequst));
        }

        tabActionGroupUI.Initialize(actionList);
        tabActionGroupUI.SetPosition(elementPos);
        tabActionGroupUI.Show();
    }

    private void InviteForLobby()
    {
        InviteLobbyManager.Instance.SendLobbyInviteRequest(selectedPlayerID);

        HideActionMenu();
    }

    private void AcceptRequst()
    {
        InviteLobbyManager.Instance.SendLobbyInviteResponse(selectedPlayerID);

        HideActionMenu();
    }

    private void DeleteFromFriend()
    {
        InformationUI.Instance.Show("Wait...", false);

        FriendServiceManager.Instance.TryDeleteFriend(selectedPlayerID)
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
