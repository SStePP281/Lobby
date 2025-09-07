using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SwitchButton : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent<bool> OnStateChanged;

    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private bool isActive = true;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateImageSprite();

        OnStateChanged.Invoke(isActive);
    }

    private void ToggleState()
    {
        isActive = !isActive;

        UpdateImageSprite();

        OnStateChanged.Invoke(isActive);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ToggleState();
        }
    }

    private void UpdateImageSprite()
    {
        image.sprite = isActive ? activeSprite : inactiveSprite;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        UpdateImageSprite();
    }
#endif
}
