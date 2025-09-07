using System;
using UnityEngine;
using Zenject;

public class PlayerDataManager : IDisposable
{
    public event Action OnPlayerDataChanged;

    [Inject] private FriendAvatarListSO avatarListSO;

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

    private FriendActivity myFriendActivity;

    private PlayerDataManager()
    {
        myFriendActivity = new FriendActivity();

        FriendServiceManager.Instance.OnUserDataUpdate += PlayerDataChanged;
        AuthenticationServiceManager.Instance.OnPlayerInfoChanged += PlayerDataChanged;
        AuthenticationServiceManager.Instance.OnSignIn += PlayerDataChanged;
    }

    public void SetNewAvatar(int spriteIndex, Action<bool, string> callback = null)
    {
        myFriendActivity.AvatarIndex = spriteIndex;

        FriendServiceManager.Instance.TrySetPresenceActivity(myFriendActivity, callback);
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

    private void PlayerDataChanged() => OnPlayerDataChanged?.Invoke();

    public void Dispose()
    {
        instance = null;

        isDisposing = true;

        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnPlayerInfoChanged -= PlayerDataChanged;
            AuthenticationServiceManager.Instance.OnSignIn -= PlayerDataChanged;
        }

        if (FriendServiceManager.Instance != null)
        {
            FriendServiceManager.Instance.OnUserDataUpdate -= PlayerDataChanged;
        }
    }
}
