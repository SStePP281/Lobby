using ServiceRequest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class FriendIncomingRequestMenuUI : UIList<FriendElementUI, FriendData>
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
        return FriendServiceManager.Instance.GetIncomingRequestsList()
                                            .OrderBy(f => !f.InLobby)
                                            .ThenBy(f => f.Type != FriendData.RequestType.WasInvited)
                                            .ThenBy(f => f.Type != FriendData.RequestType.MyInvited)
                                            .ThenByDescending(f => f.IsOnline)
                                            .ToList(); ;
    }

    protected override List<TabNameAction> GetActions()
    {
        return new List<TabNameAction>
        {
            new TabNameAction("Profile", ShowProfile),
            new TabNameAction("Accept", AcceptRequest),
            new TabNameAction("Decline", DeclineRequest)
        };
    }

    //Actions
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
