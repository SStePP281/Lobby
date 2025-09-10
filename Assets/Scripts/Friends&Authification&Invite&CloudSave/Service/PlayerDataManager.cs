using System;
using System.Collections.Generic;
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

    private void FriendServiceManager_OnInitialize() => FriendServiceManager.Instance.TrySetPresenceActivity(myFriendActivity);

    public void SetFriendAvatarListSO(FriendAvatarListSO lits) => avatarListSO = lits;

    public void SetNewAvatar(int spriteIndex, Action<bool, string> callback = null)
    {
        myFriendActivity.AvatarIndex = spriteIndex;

        FriendServiceManager.Instance.TrySetPresenceActivity(myFriendActivity, (bool s, string m) =>
        {
            callback?.Invoke(s, m);

            if (s)
            {
                CloudSaveServiceManager.Instance.SavePlayerData(CloudSaveData.AVATAR_DATA, spriteIndex, false);
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
        if (myFriendDict.ContainsKey(friendID))
        {
            return avatarListSO.GetAvatarSpriteByIndex(myFriendDict[friendID].AvatarIndex);
        }
        else
        {
            CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.AVATAR_DATA, (bool succes, Item item) => 
            {
                if (succes)
                {
                    myFriendDict[friendID] = new FriendActivity(item.Value.GetAs<int>());

                    FriendServiceManager.Instance.TryForceRefresh();
                }
            }, 
            friendID);

            return null;
        }
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
