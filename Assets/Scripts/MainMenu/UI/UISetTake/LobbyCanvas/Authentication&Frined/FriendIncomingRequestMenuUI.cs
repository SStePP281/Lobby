using ServiceRequest;
using System.Collections.Generic;
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
        return FriendServiceManager.Instance.GetIncomingRequestsList();
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
        FriendServiceManager.Instance.TryAddFriend(selectedPlayerID)
            .OnStartRun(() => InformationUI.Instance.Show("Wait...", false))
            .OnSuccessComplete(() => InformationUI.Instance.Hide())
            .OnFailureComplete(m => InformationUI.Instance.Show(m))
            .Start();

        HideActionMenu();
    }

    private void DeclineRequest()
    {
        FriendServiceManager.Instance.TryDeleteIncomingRequest(selectedPlayerID)
            .OnStartRun(() => InformationUI.Instance.Show("Wait...", false))
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
