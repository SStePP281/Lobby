using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIList<T, D> : MonoBehaviour where D : IUserData 
                                                   where T : BaseUIListElement<D>
{
    [Header("Reference")]
    [SerializeField] protected Transform container;
    [SerializeField] protected TabActionGroupUI tabActionGroupUI;
    [SerializeField] private TextMeshProUGUI emptyText;

    protected ObjectPool<T> objectPool;
    protected string selectedPlayerID = string.Empty;

    protected virtual void Awake()
    {
        objectPool = new ObjectPool<T>(CreateUIElement, container);
    }

    protected virtual void Start()
    {
        MenuInput.Instance.OnEscepeClick += MenuInput_OnEscepeClick;
        MenuInput.Instance.OnMouseClick += MenuInput_OnMouseClick;
    }

    protected abstract T CreateUIElement(Transform container);

    protected abstract List<D> GetDataList();

    protected abstract void OnElementClick(Vector2 elementPos, string playerID);

    protected void RefreshFriendData()
    {
        List<D> list = GetDataList();

        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(list.Count == 0);
        }

        foreach (D data in list)
        {
            T elementUI = objectPool.GetElement();
            elementUI.Initialize(data);
            elementUI.OnClick -= OnElementClick;
            elementUI.OnClick += OnElementClick;
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

    protected virtual void OnDisable()
    {
        tabActionGroupUI.Hide();
    }

    protected virtual void OnEnable()
    {
        RefreshFriendData();
    }

    protected virtual void OnDestroy()
    {
        objectPool.Clear();

        if (MenuInput.Instance != null)
        {
            MenuInput.Instance.OnEscepeClick -= MenuInput_OnEscepeClick;
            MenuInput.Instance.OnMouseClick -= MenuInput_OnMouseClick;
        }
    }
}