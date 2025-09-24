using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseUIListElement<D> : MonoBehaviour, IPointerClickHandler where D : IUserData
{
    public event Action<Vector2, string> OnClick;

    private D data;

    public virtual BaseUIListElement<D> Initialize(D data)
    {
        this.data = data;

        return this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnClick?.Invoke(transform.position, data.UserID);
        }
    }
}