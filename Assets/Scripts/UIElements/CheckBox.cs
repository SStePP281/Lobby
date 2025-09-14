using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CheckBox : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent<bool> OnStateChanged;

    [Header("Reference")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;

    [Header("Settings")]
    [SerializeField] private bool startActive = true;
    [SerializeField] private bool saveState;

    private Image image;
    private bool isActive;

    private void Awake()
    {
        image = GetComponent<Image>();
        isActive = startActive;
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

    private void OnDisable()
    {
        if (saveState) { return; }

        isActive = !startActive;
        ToggleState();
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
