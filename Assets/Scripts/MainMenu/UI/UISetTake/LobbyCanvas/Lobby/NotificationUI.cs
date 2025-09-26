using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    [Inject] private INotificationElementFactory elementFactory;

    [Header("Reference")]
    [SerializeField] private Transform container;

    [Header("Settings")]
    [SerializeField] private int maxElementCount = 5;
    [SerializeField] private float showTime = 3.0f;

    private ObjectPool<NotificationElementUI> objectPool;

    private LinkedList<NotificationElementUI> elements;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        objectPool = new ObjectPool<NotificationElementUI>(GetElement, container);
        elements = new LinkedList<NotificationElementUI>();
    }

    public void CreateNotification(FriendData friendData, string message)
    {
        var node = FindNode(friendData);
        if (node != null)
        {
            node.Value.RestartDestroyTimer(showTime);

            elements.Remove(node);
            elements.AddLast(node);

            return;
        }

        if (elements.Count >= maxElementCount)
        {
            var oldest = elements.First.Value;
            elements.RemoveFirst();

            oldest.SetOnDestroyed(() =>
            {
                objectPool.Hide(oldest);
            });

            oldest.StartDestroyTimer(0f);
        }

        NotificationElementUI newElement = objectPool.GetElement().Initialize(friendData, message);

        newElement.SetOnDestroyed(() =>
        {
            elements.Remove(newElement);
            objectPool.Hide(newElement);
        })
        .StartDestroyTimer(showTime);

        elements.AddLast(newElement);
    }

    private LinkedListNode<NotificationElementUI> FindNode(FriendData friendData)
    {
        var current = elements.First;
        while (current != null)
        {
            if (current.Value.Data.UserID == friendData.UserID)
                return current;
            current = current.Next;
        }
        return null;
    }

    private NotificationElementUI GetElement(Transform parent)
    {
        return elementFactory.CreateElement(parent);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        objectPool.Clear();
        elements.Clear();
    }
}
