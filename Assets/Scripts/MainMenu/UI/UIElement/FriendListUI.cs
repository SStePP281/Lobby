using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public abstract class FriendListUI : MonoBehaviour
{
    [Inject] private IFriendElementFactory friendElementFactory;

    [Header("Reference")]
    [SerializeField] private Transform container;
    [SerializeField] protected TabActionGroupUI tabActionGroupUI;
    [SerializeField] private TextMeshProUGUI emptyText;

    protected ObjectPool<FriendElementUI> objectPool;
    protected string selectedPlayerID = string.Empty;

    protected virtual void Awake()
    {
        objectPool = new ObjectPool<FriendElementUI>(friendElementFactory.CreateFriendElement, container);

        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
        HashDataManager.Instance.OnFriendDataChanged += HashDataManager_OnFriendDataChanged;
    }

    private void Start()
    {
        MenuInput.Instance.OnEscepeClick += MenuInput_OnEscepeClick;
        MenuInput.Instance.OnMouseClick += MenuInput_OnMouseClick;
    }

    protected abstract List<FriendData> GetFriendList();

    public virtual void RefreshFriendData()
    {
        HideActionMenu();

        objectPool.Hide();

        List<FriendData> friends = GetFriendList()
            .OrderBy(f => !f.InLobby)
            .ThenBy(f => f.Type != FriendData.RequestType.WasInvited)
            .ThenBy(f => f.Type != FriendData.RequestType.MyInvited)
            .ThenByDescending(f => f.IsOnline)
            .ToList();

        emptyText.gameObject.SetActive(friends.Count == 0);

        foreach (FriendData friend in friends)
        {
            var elementUI = objectPool.GetElement();
            elementUI.Initialize(friend);
            elementUI.OnClick -= OnFriendElementClick;
            elementUI.OnClick += OnFriendElementClick;
        }
    }

    protected abstract void OnFriendElementClick(Vector2 elementPos, string playerID);

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

    protected void HideActionMenu()
    {
        tabActionGroupUI.Hide();
        selectedPlayerID = string.Empty;
    }

    protected virtual void AuthenticationServiceManager_OnSignOut()
    {
        objectPool.Clear();
        selectedPlayerID = string.Empty;
    }

    protected virtual void HashDataManager_OnFriendDataChanged() => RefreshFriendData();

    private void OnDisable()
    {
        tabActionGroupUI.Hide();
    }

    private void OnEnable()
    {
        RefreshFriendData();
    }

    protected virtual void OnDestroy()
    {
        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnFriendDataChanged -= HashDataManager_OnFriendDataChanged;
        }

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }

        if (MenuInput.Instance != null)
        {
            MenuInput.Instance.OnEscepeClick -= MenuInput_OnEscepeClick;
            MenuInput.Instance.OnMouseClick -= MenuInput_OnMouseClick;
        }

        objectPool.Clear();
    }
}
