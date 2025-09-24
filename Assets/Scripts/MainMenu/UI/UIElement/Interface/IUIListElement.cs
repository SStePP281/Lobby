using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseUIListElement<D> : MonoBehaviour, IPointerClickHandler where D : IUserData
{
    public event Action<Vector2, string> OnClick;

    private D userData;

    public virtual BaseUIListElement<D> Initialize(D data)
    {
        userData = data;

        return this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnClick?.Invoke(transform.position, userData.UserID);
        }
    }
}