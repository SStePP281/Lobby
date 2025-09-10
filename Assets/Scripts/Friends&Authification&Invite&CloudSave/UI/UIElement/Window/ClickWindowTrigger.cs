using UnityEngine;
using UnityEngine.EventSystems;

public class ClickWindowTrigger : BaseButtonSelector
{
    [Header("Reference")]
    [SerializeField] private GameObject baseWindowUI;

    [Header("Settings")]
    [SerializeField] private bool needOpen = true;

    private IBaseWindowUI window;

    protected override void Awake()
    {
        base.Awake();

        window = baseWindowUI.GetComponent<IBaseWindowUI>();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        if (needOpen)
        {
            window.Open();
        }
        else
        {
            window.Close();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (baseWindowUI != null)
        {
            if (baseWindowUI.GetComponent<IBaseWindowUI>() == null)
            {
                baseWindowUI = null;
            }
        }
    }
#endif
}


