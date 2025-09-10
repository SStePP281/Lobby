using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvatarElementUI : BaseButtonSelector
{
    [SerializeField] private Image avatarImage;

    private event Action<int> OnAvatarClick;
    private int index;

    public AvatarElementUI Initialize(Sprite sprite, int index, Action<int> OnClick)
    {
        avatarImage.sprite = sprite;
        OnAvatarClick = OnClick;
        this.index = index;

        return this;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        
        OnAvatarClick?.Invoke(index);
    }
}
