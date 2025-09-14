using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;
using UnityEngine;

public class PlayerDataManager : IDisposable
{
    public event Action OnPlayerDataChanged;

    private static bool isDisposing = false;
    private static PlayerDataManager instance;
    public static PlayerDataManager Instance
    {
        get
        {
            if (isDisposing) { return null; }

            if (instance == null)
            {
                instance = new PlayerDataManager();
            }

            return instance;
        }
    }

    private FriendAvatarListSO avatarListSO;
    private FriendActivity myFriendActivity;

    private Dictionary<string, FriendActivity> myFriendDict;

    private PlayerDataManager()
    {
        myFriendActivity = new FriendActivity();
        myFriendDict = new Dictionary<string, FriendActivity>();

        FriendServiceManager.Instance.OnUserDataUpdate += OnUserDataUpdate;
        FriendServiceManager.Instance.OnFriendListChanged += FriendServiceManager_OnFriendListChanged;

        AuthenticationServiceManager.Instance.OnPlayerInfoChanged += OnUserDataUpdate;
        AuthenticationServiceManager.Instance.OnSignIn += AuthenticationServiceManager_OnSignIn;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
    }

    public void SetFriendAvatarListSO(FriendAvatarListSO lits) => avatarListSO = lits;

    public string GetMyUserID() =>  AuthenticationServiceManager.Instance.GetUserId();

    public string GetMyUserName() => AuthenticationServiceManager.Instance.GetPlayerName();

    public FriendActivity GetMyFriendActivity() => myFriendActivity;

    public void SetMyFriendActivity(FriendActivity friendActivity, Action<bool, string> callback = null)
    {
        myFriendActivity = friendActivity;

        FriendServiceManager.Instance.TrySetPresenceActivity(myFriendActivity, (bool s, string m) =>
        {
            callback?.Invoke(s, m);

            if (s)
            {
                CloudSaveServiceManager.Instance.SavePlayerData(CloudSaveData.FRIEND_ACTIVITY_DATA, myFriendActivity, true);
            }
        });
    }

    public FriendActivity GetFriendActivity(string friendID)
    {
        if (myFriendDict.ContainsKey(friendID))
        {
            return myFriendDict[friendID];
        }

        return null;
    }

    private async Task LoadFriendOfflineData(Action callback = null)
    {
        List<FriendData> friendList = FriendServiceManager.Instance.GetFriendList()
                 .Concat(FriendServiceManager.Instance.GetIncomingRequestsList())
                 .ToList();

        List<FriendData> offlineFriendList = new List<FriendData>();
        foreach (FriendData friendData in friendList)
        {
            if (friendData.IsOnline)
            {
                if (!myFriendDict.ContainsKey(friendData.FriendID))
                {
                    myFriendDict[friendData.FriendID] = new FriendActivity();
                }

                if (friendData.Avatar != null)
                {
                    myFriendDict[friendData.FriendID].AvatarIndex = avatarListSO.GetIndexBySprite(friendData.Avatar);
                }
            }
            else
            {
                offlineFriendList.Add(friendData);
            }
        }

        if (offlineFriendList.Count == 0)
        {
            callback?.Invoke();
            return;
        }

        var tcs = new TaskCompletionSource<bool>();
        int remaining = offlineFriendList.Count;

        foreach (FriendData friendData in offlineFriendList)
        {
            CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.FRIEND_ACTIVITY_DATA, (bool success, Item item) =>
            {
                if (success)
                {
                    myFriendDict[friendData.FriendID] = item.Value.GetAs<FriendActivity>();
                }
                else
                {
                    if (!myFriendDict.ContainsKey(friendData.FriendID))
                    {
                        myFriendDict[friendData.FriendID] = new FriendActivity();
                    }
                }

                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    tcs.SetResult(true);
                }
            },
            friendData.FriendID);
        }

        await tcs.Task;

        callback?.Invoke();
    }

    private void LoadStartData()
    {
        CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.FRIEND_ACTIVITY_DATA, (bool s, Item item) =>
        {
            if (s)
            {
                myFriendActivity = item.Value.GetAs<FriendActivity>();
                FriendServiceManager.Instance.TrySetPresenceActivity(myFriendActivity);
                OnUserDataUpdate();
            }
        });

        _ = LoadFriendOfflineData(OnUserDataUpdate);
    }

    private void OnUserDataUpdate() => OnPlayerDataChanged?.Invoke();

    private void FriendServiceManager_OnFriendListChanged()
    {
        List<FriendData> friendList = FriendServiceManager.Instance.GetFriendList()
                 .Concat(FriendServiceManager.Instance.GetIncomingRequestsList())
                 .ToList();

        foreach (FriendData friendData in friendList)
        {
            if (!myFriendDict.ContainsKey(friendData.FriendID))
            {
                if (friendData.Avatar != null)
                {
                    myFriendDict[friendData.FriendID] = new FriendActivity(avatarListSO.GetIndexBySprite(friendData.Avatar));
                }
                else
                {
                    CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.FRIEND_ACTIVITY_DATA, (bool s, Item item) =>
                    {
                        if (s)
                        {
                            myFriendDict[friendData.FriendID] = item.Value.GetAs<FriendActivity>();
                        }
                    },
                    friendData.FriendID);
                }
            }
            else
            {
                if (friendData.Avatar != null)
                {
                    myFriendDict[friendData.FriendID].AvatarIndex = avatarListSO.GetIndexBySprite(friendData.Avatar);
                }
            }
        }
    }

    private void FriendServiceManager_OnInitialize()
    {
        LoadStartData();

        FriendServiceManager.Instance.OnInitialize -= FriendServiceManager_OnInitialize;
    }

    private void AuthenticationServiceManager_OnSignIn()
    {
        if (!FriendServiceManager.Instance.IsInitialize())
        {
            FriendServiceManager.Instance.OnInitialize += FriendServiceManager_OnInitialize;
            return;
        }

        LoadStartData();
    }

    private void AuthenticationServiceManager_OnSignOut()
    {
        myFriendActivity = new FriendActivity();
        myFriendDict.Clear();
    }

    public void Dispose()
    {
        instance = null;

        isDisposing = true;

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnPlayerInfoChanged -= OnUserDataUpdate;
            AuthenticationServiceManager.Instance.OnSignIn -= AuthenticationServiceManager_OnSignIn;
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }

        if (FriendServiceManager.Instance != null)
        {
            FriendServiceManager.Instance.OnUserDataUpdate -= OnUserDataUpdate;
            FriendServiceManager.Instance.OnFriendListChanged -= FriendServiceManager_OnFriendListChanged;
        }
    }
}
