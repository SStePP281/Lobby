using System.Collections.Generic;
using UnityEngine;

public class TabPageGroupUI : TabGroupUI
{
    [SerializeField] private List<Transform> pageList;

    protected override void Start()
    {
        base.Start();

        OnTabSelected(tabButtonList[0]);
    }

    public override void OnTabSelected(TabButtonUI button)
    {
        base.OnTabSelected(button);

        int tabIndex = button.transform.GetSiblingIndex();
        for (int i = 0; i < pageList.Count; i++)
        {
            pageList[i].gameObject.SetActive(tabIndex == i);
        }
    }
}
