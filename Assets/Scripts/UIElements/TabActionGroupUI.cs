using System;
using System.Collections.Generic;
using UnityEngine;

public struct TabNameAction
{
    public TabNameAction(string name, Action OnClick)
    {
        Name = name;
        OnSelected = OnClick;
    }

    public string Name;
    public Action OnSelected;
}

public class TabActionGroupUI : TabGroupUI
{
    private List<TabNameAction> onTabClick;
    private RectTransform canvasRect;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        onTabClick = new List<TabNameAction>();

        canvasRect = GetComponentInParent<Canvas>().transform as RectTransform;
    }

    protected override void Start()
    {
        base.Start();

        Hide();
    }

    public void Initialize(List<TabNameAction> tabAction)
    {
        onTabClick.Clear();

        onTabClick = tabAction;

        if (tabAction.Count > tabButtonList.Count)
        {
            if (tabButtonList.Count == 0) { return; }

            int needCreate = tabAction.Count - tabButtonList.Count;

            for (int i = 0; i < needCreate; i++)
            {
                TabButtonUI tabButtonUI = Instantiate(tabButtonList[0], transform);
                tabButtonList.Add(tabButtonUI);
            }
        }

        for (int i = 0; i < tabAction.Count; i++)
        {
            tabButtonList[i].text.text = tabAction[i].Name;

            tabButtonList[i].gameObject.SetActive(true);
        }

        ClearSelecterTab();

        if (tabAction.Count == tabButtonList.Count) { return; }

        for (int i = tabAction.Count; i < tabButtonList.Count; i++)
        {
            tabButtonList[i].gameObject.SetActive(false);
        }
    }

    public override void OnTabSelected(TabButtonUI button)
    {
        base.OnTabSelected(button);

        int index = tabButtonList.IndexOf(button);

        onTabClick[index].OnSelected?.Invoke();
    }

    public void SetPosition(Vector2 screenPosition) => transform.position = screenPosition;
}