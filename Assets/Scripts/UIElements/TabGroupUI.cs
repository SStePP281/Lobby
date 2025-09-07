using System.Collections.Generic;
using UnityEngine;

public class TabGroupUI : MonoBehaviour
{
    [SerializeField] protected Sprite tabIdel;
    [SerializeField] protected Sprite tabHover;
    [SerializeField] protected Sprite tabActive;

    protected List<TabButtonUI> tabButtonList;
    protected TabButtonUI selectedTab;

    protected virtual void Start()
    {
        tabButtonList.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
    }

    public void Subscribe(TabButtonUI button)
    {
        if (tabButtonList == null)
        {
            tabButtonList = new List<TabButtonUI>();
        }

        if (tabButtonList.Contains(button)) { return; }

        tabButtonList.Add(button);
    }

    public void OnTabEnter(TabButtonUI button)
    {
        ResetTabs();

        if (selectedTab == null || selectedTab != button)
        {
            button.background.sprite = tabHover;
        }
    }

    public void OnTabExit(TabButtonUI button)
    {
        ResetTabs();
    }

    public virtual void OnTabSelected(TabButtonUI button)
    {
        selectedTab = button;
        ResetTabs();
        button.background.sprite = tabActive;
    }

    private void ResetTabs()
    {
        foreach (TabButtonUI button in tabButtonList)
        {
            if (selectedTab != null && button == selectedTab) { continue; }

            button.background.sprite = tabIdel;
        }
    }

    protected void ClearSelecterTab()
    {
        foreach (TabButtonUI button in tabButtonList)
        {
            button.background.sprite = tabIdel;
        }
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public bool IsActive() => gameObject.activeSelf;
}
