using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public abstract class FriendListUI : MonoBehaviour
{
    [Inject] private IFriendElementFactory friendElementFactory;

    [SerializeField] private Transform container;
    [SerializeField] protected TabActionGroupUI tabActionGroupUI;
    [SerializeField] private TextMeshProUGUI emptyText;

    protected ObjectPool<FriendElementUI> objectPool;
    protected string selectedPlayerID = string.Empty;

    protected virtual void Awake()
    {
        objectPool = new ObjectPool<FriendElementUI>(friendElementFactory.CreateFriendElement, container);

        FriendServiceManager.Instance.OnInitialize += OnFriendListChanged;
        FriendServiceManager.Instance.OnFriendListChanged += OnFriendListChanged;
    }

    private void Start()
    {
        MenuInput.Instance.OnEscepeClick += MenuInput_OnEscepeClick;
        MenuInput.Instance.OnMouseClick += MenuInput_OnMouseClick;
    }

    protected abstract List<FriendData> GetFriendList();

    protected virtual void OnFriendListChanged() => RefreshFriendData();

    public void RefreshFriendData()
    {
        objectPool.Hide();

        var friends = GetFriendList();

        emptyText.gameObject.SetActive(friends.Count == 0);

        foreach (var friend in friends)
        {
            var elementUI = objectPool.GetElement();
            elementUI.Initaliaze(friend);
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


    private void HideActionMenu()
    {
        tabActionGroupUI.Hide();
        selectedPlayerID = string.Empty;
    }

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
        if (FriendServiceManager.Instance != null)
        {
            FriendServiceManager.Instance.OnInitialize -= OnFriendListChanged;
            FriendServiceManager.Instance.OnFriendListChanged -= OnFriendListChanged;
        }

        if (MenuInput.Instance != null)
        {
            MenuInput.Instance.OnEscepeClick -= MenuInput_OnEscepeClick;
            MenuInput.Instance.OnMouseClick -= MenuInput_OnMouseClick;
        }

        objectPool.Clear();
    }
}
