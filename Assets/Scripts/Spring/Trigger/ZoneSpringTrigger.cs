using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneSpringTrigger : BaseSpringTrigger, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SpringState onEnter;
    [SerializeField] private SpringState onExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetNewSpringState(onEnter);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetNewSpringState(onExit);
    }
}
