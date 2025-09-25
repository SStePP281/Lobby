using UnityEngine;
using Zenject;

public class NotificationElementFactory : INotificationElementFactory
{
    [Inject] private NotificationElementUI notificationElementUIPrefab;

    public NotificationElementUI CreateElement(Transform container = null)
    {
        if (notificationElementUIPrefab  == null) { return null; }

        return Object.Instantiate(notificationElementUIPrefab, container);
    }
}