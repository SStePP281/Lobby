using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class NotificationInviteListUI : MonoBehaviour
{
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
        objectPool = new ObjectPool<NotificationElementUI>(GetElement, container);
        elements = new LinkedList<NotificationElementUI>();

        InviteLobbyManager.Instance.OnReceiveInvite += InviteLobbyManager_OnReceiveInvite;
        FriendServiceManager.Instance.OnReceiveFriendRequest += FriendServiceManager_OnReceiveFriendRequest;
    }

    private void CreateNotification(FriendData friendData, string message)
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

    private void InviteLobbyManager_OnReceiveInvite(string userID) => CreateNotification(FriendServiceManager.Instance.GetFriendData(userID), "send invite");

    private void FriendServiceManager_OnReceiveFriendRequest(string userID) => CreateNotification(FriendServiceManager.Instance.GetFriendData(userID), "new request");

    private void OnDestroy()
    {
        if (InviteLobbyManager.Instance != null)
        {
            InviteLobbyManager.Instance.OnReceiveInvite -= InviteLobbyManager_OnReceiveInvite;
        }

        if (FriendServiceManager.Instance != null)
        {
            FriendServiceManager.Instance.OnReceiveFriendRequest -= FriendServiceManager_OnReceiveFriendRequest;
        }

        objectPool.Clear();
        elements.Clear();
    }
}
