using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterElementUI : BaseButtonSelector
{
    [Header("Reference")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI chracterNameText;
    [SerializeField] private Outline selectedOutline;

    private event Action<int> OnAvatarClick;
    private int index;

    public CharacterElementUI Initialize(Sprite sprite, string characterName, int index, Action<int> OnClick)
    {
        avatarImage.sprite = sprite;
        chracterNameText.text = characterName;
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

    public void SetSelected(bool isSelected) => selectedOutline.enabled = isSelected;
}
