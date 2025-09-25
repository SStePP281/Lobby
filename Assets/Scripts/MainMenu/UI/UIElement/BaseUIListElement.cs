using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseUIListElement<D> : MonoBehaviour, IPointerClickHandler where D : IUserData
{
    public event Action<Vector2, string> OnClick;

    public D Data {get; private set;}

    public virtual BaseUIListElement<D> Initialize(D data)
    {
        Data = data;

        return this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnClick?.Invoke(transform.position, Data.UserID);
        }
    }
}