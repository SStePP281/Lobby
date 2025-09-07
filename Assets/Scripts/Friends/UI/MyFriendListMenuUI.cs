using System.Collections.Generic;
using System.Linq;
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

        if (FriendServiceManager.Instance.GetFriendList().Where(f => f.FriendID == playerID).First().IsOnline)
        {
            actionList.Insert(1, new TabNameAction("Invite", InviteForLobby));
        }

        if (InviteLobbyManager.Instance.GetRequstType(playerID) == FriendData.RequestType.WasInvited)
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

        tabActionGroupUI.Hide();
    }

    private void AcceptRequst()
    {
        InviteLobbyManager.Instance.SendLobbyInviteResponse(selectedPlayerID);

        tabActionGroupUI.Hide();
    }

    private void DeleteFromFriend()
    {
        if (FriendServiceManager.Instance.TryDeleteFriend(selectedPlayerID, FriendServiceManager_Callback))
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
