using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BaseButtonSelector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Reference")]
    [SerializeField] private Image selectedImage;

    [Header("Select")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color enterColor = Color.white;
    [SerializeField] private Color clickColor = Color.white;

    private bool isEnter;
    private bool isClicked;

    protected virtual void Awake()
    {
        if (selectedImage == null)
        {
            selectedImage = GetComponent<Image>();
        }

        UpdateVisualState(false, false);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        isClicked = true;
        isEnter = true;

        UpdateVisualState(isClicked, isEnter);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        isClicked = false;

        UpdateVisualState(isClicked, isEnter);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        isEnter = true;

        UpdateVisualState(isClicked, isEnter);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        isEnter = false;

        UpdateVisualState(isClicked, isEnter);
    }

    private void UpdateVisualState(bool isClicked, bool isEnter)
    {
        if (isClicked)
        {
            selectedImage.color = clickColor;
        }
        else if (isEnter)
        {
            selectedImage.color = enterColor;
        }
        else
        {
            selectedImage.color = normalColor;
        }
    }
}