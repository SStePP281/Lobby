using ServiceRequest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class LobbyUserListUI : UIList<LobbyElementUI, LobbyUserData>
{
    [Inject] private ILobbyElementFactory lobbyElementFactory;

    protected override void Awake()
    {
        base.Awake();

        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
        LobbyServiceManager.Instance.OnLobbyGeneralDataUpdate += LobbyServiceManager_OnLobbyGeneralDataUpdate;
        HashDataManager.Instance.OnUserDataChanged += HashDataManager_OnUserDataChanged;
        HashDataManager.Instance.OnFriendDataChanged += HashDataManager_OnFriendDataChanged;
    }

    protected override LobbyElementUI CreateUIElement(Transform container)
    {
        return lobbyElementFactory.CreateElement(container);
    }

    protected override List<LobbyUserData> GetDataList()
    {
        return LobbyServiceManager.Instance.GetLobbyUserList()
                                           .OrderBy(l => !l.IsMyData).ToList();
    }

    protected override List<TabNameAction> GetActions()
    {
        List<TabNameAction> actionList = new List<TabNameAction>
        {
            new TabNameAction("Profile", ShowProfile),
        };

        if (LobbyServiceManager.Instance.IsMyLobby())
        {
            actionList.Add(new TabNameAction("Migrate host", MigrateHost));
            actionList.Add(new TabNameAction("Kick", KickUser));
        }

        return actionList;
    }

    //Actions
    private void KickUser()
    {
        LobbyServiceManager.Instance.KickClientFromLobby(selectedPlayerID)
                                    .OnStartRun(() => InformationUI.Instance.Show("Wait...", false))
                                    .OnSuccessComplete(() => InformationUI.Instance.Hide())
                                    .OnFailureComplete(m => InformationUI.Instance.Show(m))
                                    .Start();

        HideActionMenu();
    }

    private void MigrateHost()
    {
        LobbyServiceManager.Instance.MigrateLobbyHost(selectedPlayerID)
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
    private void LobbyServiceManager_OnLobbyGeneralDataUpdate() => RefreshFriendData();

    private void AuthenticationServiceManager_OnSignOut()
    {
        objectPool.Clear();
        selectedPlayerID = string.Empty;
    }

    private void HashDataManager_OnFriendDataChanged() => RefreshFriendData();

    private void HashDataManager_OnUserDataChanged() => RefreshFriendData();


    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (LobbyServiceManager.Instance != null)
        {
            LobbyServiceManager.Instance.OnLobbyGeneralDataUpdate -= LobbyServiceManager_OnLobbyGeneralDataUpdate;
        }

        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnUserDataChanged -= HashDataManager_OnUserDataChanged;
            HashDataManager.Instance.OnFriendDataChanged -= HashDataManager_OnFriendDataChanged;
        }

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }
    }
}
