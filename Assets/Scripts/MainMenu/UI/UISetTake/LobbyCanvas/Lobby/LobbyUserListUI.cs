using ServiceRequest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class LobbyUserListUI : MonoBehaviour
{
    [Inject] private ILobbyElementFactory lobbyElementFactory;

    [Header("Reference")]
    [SerializeField] private Transform container;
    [SerializeField] private TabActionGroupUI tabActionGroupUI;

    private ObjectPool<LobbyElementUI> objectPool;
    private string selectedPlayerID = string.Empty;

    private void Awake()
    {
        objectPool = new ObjectPool<LobbyElementUI>(lobbyElementFactory.CreateElement, container);

        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
        LobbyServiceManager.Instance.OnLobbyGeneralDataUpdate += LobbyServiceManager_OnLobbyGeneralDataUpdate;
        HashDataManager.Instance.OnUserDataChanged += HashDataManager_OnUserDataChanged;
        HashDataManager.Instance.OnFriendDataChanged += HashDataManager_OnFriendDataChanged;
    }

    private void Start()
    {
        MenuInput.Instance.OnEscepeClick += MenuInput_OnEscepeClick;
        MenuInput.Instance.OnMouseClick += MenuInput_OnMouseClick;
    }

    public void RefreshFriendData()
    {
        objectPool.Hide();

        List<LobbyUserData> lobbyUserDataList = LobbyServiceManager.Instance.GetLobbyUserList()
                                                                   .OrderBy(l => !l.IsMyData).ToList();

        foreach (LobbyUserData data in lobbyUserDataList)
        {
            var elementUI = objectPool.GetElement();
            elementUI.Initialize(data);
            elementUI.OnClick -= OnFriendElementClick;
            elementUI.OnClick += OnFriendElementClick;
        }
    }

    private void ShowProfile()
    {
        HideActionMenu();
    }

    private void KickUser()
    {
        InformationUI.Instance.Show("Wait...", false);

        LobbyServiceManager.Instance.KickClientFromLobby(selectedPlayerID)
                                    .OnSuccessComplete(() => InformationUI.Instance.Hide())
                                    .OnFailureComplete(m => InformationUI.Instance.Show(m))
                                    .Start();

        HideActionMenu();
    }

    private void MigrateHost()
    {
        InformationUI.Instance.Show("Wait...", false);

        LobbyServiceManager.Instance.MigrateLobbyHost(selectedPlayerID)
                                    .OnSuccessComplete(() => InformationUI.Instance.Hide())
                                    .OnFailureComplete(m => InformationUI.Instance.Show(m))
                                    .Start();

        HideActionMenu();
    }

    private void HideActionMenu()
    {
        tabActionGroupUI.Hide();
        selectedPlayerID = string.Empty;
    }

    private void OnFriendElementClick(Vector2 elementPos, string playerID)
    {
        selectedPlayerID = playerID;

        if (selectedPlayerID == AuthenticationServiceManager.Instance.GetUserId())
        { 
            HideActionMenu();
            return;
        }

        List<TabNameAction> actionList = new List<TabNameAction>
        {
            new TabNameAction("Profile", ShowProfile),
        };

        if (LobbyServiceManager.Instance.IsMyLobby())
        {
            actionList.Insert(1, new TabNameAction("Migrate host", MigrateHost));
            actionList.Add(new TabNameAction("Kick", KickUser));
        }

        tabActionGroupUI.Initialize(actionList);
        tabActionGroupUI.SetPosition(elementPos);
        tabActionGroupUI.Show();
    }

    private void MenuInput_OnEscepeClick()
    {
        if (!tabActionGroupUI.IsActive()) { return; }

        HideActionMenu();
    }

    private void MenuInput_OnMouseClick()
    {
        if (!tabActionGroupUI.IsActive()) { return; }

        if (!IsPointerOverUIElement())
        {
            HideActionMenu();
        }
    }

    private bool IsPointerOverUIElement()
    {
        if (EventSystem.current == null) return false;

        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject == tabActionGroupUI.gameObject ||
                result.gameObject.transform.IsChildOf(tabActionGroupUI.transform))
            {
                return true;
            }
        }

        return false;
    }

    private void LobbyServiceManager_OnLobbyGeneralDataUpdate() => RefreshFriendData();

    private void AuthenticationServiceManager_OnSignOut()
    {
        objectPool.Clear();
        selectedPlayerID = string.Empty;
    }

    private void HashDataManager_OnFriendDataChanged() => RefreshFriendData();

    private void HashDataManager_OnUserDataChanged() => RefreshFriendData();

    private void OnDisable()
    {
        HideActionMenu();
    }

    private void OnEnable()
    {
        RefreshFriendData();
    }

    private void OnDestroy()
    {
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

        objectPool.Clear();
    }
}
