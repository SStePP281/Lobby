using UnityEngine;

public interface INotificationElementFactory
{
    public NotificationElementUI CreateElement(Transform container = null);
}