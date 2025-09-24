using ServiceRequest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class MyFriendListMenuUI : UIList<FriendElementUI, FriendData>
{
    [Inject] private IFriendElementFactory friendElementFactory;

    protected override void Awake()
    {
        base.Awake();

        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
        HashDataManager.Instance.OnFriendDataChanged += HashDataManager_OnFriendDataChanged;
    }

    protected override FriendElementUI CreateUIElement(Transform container)
    {
        return friendElementFactory.CreateFriendElement(container);
    }

    protected override List<FriendData> GetDataList()
    {
        return FriendServiceManager.Instance.GetFriendList()
                                            .OrderBy(f => !f.InLobby)
                                            .ThenBy(f => f.Type != FriendData.RequestType.WasInvited)
                                            .ThenBy(f => f.Type != FriendData.RequestType.MyInvited)
                                            .ThenByDescending(f => f.IsOnline)
                                            .ToList();
    }

    protected override List<TabNameAction> GetActions()
    {
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

        return actionList;
    }

    //Actions
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

    //Events
    private void AuthenticationServiceManager_OnSignOut()
    {
        objectPool.Clear();
        selectedPlayerID = string.Empty;
    }

    private void HashDataManager_OnFriendDataChanged() => RefreshFriendData();

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }

        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnFriendDataChanged -= HashDataManager_OnFriendDataChanged;
        }
    }
}
