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
        myFriendDict = new Dictionary<string, FriendActivity>();
        myFriendActivity = new FriendActivity();

        FriendServiceManager.Instance.OnInitialize += FriendServiceManager_OnInitialize;
        FriendServiceManager.Instance.OnUserDataUpdate += PlayerDataChanged;
        AuthenticationServiceManager.Instance.OnPlayerInfoChanged += PlayerDataChanged;
        AuthenticationServiceManager.Instance.OnSignIn += AuthenticationServiceManager_OnSignIn;
    }

    private void FriendServiceManager_OnInitialize()
    {
        CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.AVATAR_DATA, (bool s, Item item) => 
        {
            if (s)
            {
                myFriendActivity.AvatarIndex = item.Value.GetAs<int>();
                FriendServiceManager.Instance.TrySetPresenceActivity(myFriendActivity);
            }
        });

        _ = LoadFriendOfflineData(PlayerDataChanged);
    }

    public void SetFriendAvatarListSO(FriendAvatarListSO lits) => avatarListSO = lits;

    public void SetNewAvatar(int spriteIndex, Action<bool, string> callback = null)
    {
        myFriendActivity.AvatarIndex = spriteIndex;

        FriendServiceManager.Instance.TrySetPresenceActivity(myFriendActivity, (bool s, string m) =>
        {
            callback?.Invoke(s, m);

            if (s)
            {
                CloudSaveServiceManager.Instance.SavePlayerData(CloudSaveData.AVATAR_DATA, spriteIndex, true);
            }
        });
    }

    public void SetNewAvatar(Sprite sprite, Action<bool, string> callback = null)
    {
        if (sprite == null) { return; }

        SetNewAvatar(avatarListSO.GetSpriteIndexBySprite(sprite), callback);
    }

    public string GetMyUserID() =>  AuthenticationServiceManager.Instance.GetUserId();

    public string GetMyUserName() => AuthenticationServiceManager.Instance.GetPlayerName();

    public int GetMyAvatarIndex() => myFriendActivity.AvatarIndex;

    public Sprite GetMyAvatar() => avatarListSO == null ? null : avatarListSO.GetAvatarSpriteByIndex(GetMyAvatarIndex());

    public Sprite GetFriendAvatar(string friendID)
    {
        int avatarIndex = 0;

        if (myFriendDict.ContainsKey(friendID))
        {
            avatarIndex = myFriendDict[friendID].AvatarIndex;
        }

        return avatarListSO.GetAvatarSpriteByIndex(avatarIndex);
    }

    private void PlayerDataChanged() => OnPlayerDataChanged?.Invoke();

    private void AuthenticationServiceManager_OnSignIn()
    {
        PlayerDataChanged();

        CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.AVATAR_DATA, (bool s, Item item) => 
        {
            if (s)
            {
                myFriendActivity.AvatarIndex = item.Value.GetAs<int>();

                PlayerDataChanged();
            }
        });
    }

    private async Task LoadFriendOfflineData(Action callback = null)
    {
        var friendList = FriendServiceManager.Instance.GetFriendList();
        var offlineFriends = friendList.Where(f => !f.IsOnline).ToList();

        if (offlineFriends.Count == 0)
        {
            callback?.Invoke();
            return;
        }

        var tcs = new TaskCompletionSource<bool>();
        int remaining = offlineFriends.Count;

        foreach (FriendData friendData in offlineFriends)
        {
            CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.AVATAR_DATA, (bool success, Item item) =>
            {
                if (success)
                {
                    if (myFriendDict.ContainsKey(friendData.FriendID))
                    {
                        myFriendDict[friendData.FriendID].AvatarIndex = item.Value.GetAs<int>();
                    }
                    else
                    {
                        myFriendDict[friendData.FriendID] = new FriendActivity(item.Value.GetAs<int>());
                    }
                }

                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    tcs.SetResult(true);
                }
            }, friendData.FriendID);
        }

        await tcs.Task;

        callback?.Invoke();
    }

    public void Dispose()
    {
        instance = null;

        isDisposing = true;

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnPlayerInfoChanged -= PlayerDataChanged;
            AuthenticationServiceManager.Instance.OnSignIn -= AuthenticationServiceManager_OnSignIn;
        }

        if (FriendServiceManager.Instance != null)
        {
            FriendServiceManager.Instance.OnUserDataUpdate -= PlayerDataChanged;
            FriendServiceManager.Instance.OnInitialize -= FriendServiceManager_OnInitialize;
        }
    }
}
