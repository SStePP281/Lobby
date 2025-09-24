using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image background;
    public TextMeshProUGUI text;

    private TabGroupUI tabGroup;

    public void SetTabGroup(TabGroupUI tabGroupUI) => tabGroup = tabGroupUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       tabGroup.OnTabExit(this);
    }
}
