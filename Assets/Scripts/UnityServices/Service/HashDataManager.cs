using ServiceRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;

public class HashDataManager : IDisposable
{
    public event Action OnFriendDataChanged;
    public event Action OnUserDataChanged;

    private static bool isDisposing = false;
    private static HashDataManager instance;
    public static HashDataManager Instance
    {
        get
        {
            if (isDisposing) { return null; }

            if (instance == null)
            {
                instance = new HashDataManager();
            }

            return instance;
        }
    }

    private LobbyAvatarListSO avatarListSO;
    private FriendActivity myFriendActivity;
    private CharacterData myCharacterData;

    private Dictionary<string, FriendActivity> myFriendDict;

    private HashDataManager()
    {
        myFriendActivity = new FriendActivity();
        myCharacterData = new CharacterData();
        myFriendDict = new Dictionary<string, FriendActivity>();

        LobbyServiceManager.Instance.OnLobbyGeneralDataUpdate += LobbyServiceManager_OnLobbyGeneralDataUpdate;

        FriendServiceManager.Instance.OnInitialize += FriendServiceManager_OnInitialize;
        FriendServiceManager.Instance.OnUserDataUpdate += FriendServiceManager_OnUserDataUpdate;
        FriendServiceManager.Instance.OnFriendListChanged += FriendServiceManager_OnFriendListChanged;

        AuthenticationServiceManager.Instance.OnSignIn += AuthenticationServiceManager_OnSignIn;
        AuthenticationServiceManager.Instance.OnPlayerInfoChanged += AuthenticationServiceManager_OnPlayerInfoChanged; ;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
    }

    public Request SetMyFriendActivity(FriendActivity friendActivity)
    {
        Request request = new Request(async () =>
        {
            myFriendActivity = friendActivity;

            Task task1 = FriendServiceManager.Instance.TrySetPresenceActivity(myFriendActivity).StartAndWait();

            Task task2 = CloudSaveServiceManager.Instance.SavePlayerData(CloudSaveData.FRIEND_ACTIVITY_DATA, myFriendActivity, true).StartAndWait();

            await Task.WhenAll(task1, task2);
        })
        .OnSuccessComplete(() => OnUserDataChanged?.Invoke());

        return request;
    }

    public Request SetMyCharacterData(CharacterData characterData)
    {
        Request request = new Request(async () =>
        {
            myCharacterData = characterData;

            Task task1 = CloudSaveServiceManager.Instance.SavePlayerData(CloudSaveData.CHARACTER_DATA, myCharacterData, false).StartAndWait();

            Task task2 = LobbyServiceManager.Instance.UpdatePlayerData(LobbyServiceData.PLAYER_CHARACTER_FIELD, myCharacterData.CharacterIndex.ToString()).StartAndWait();

            await Task.WhenAll(task1, task2);
        })
        .OnSuccessComplete(() => OnUserDataChanged?.Invoke());

        return request;
    }

    public void SetFriendAvatarListSO(LobbyAvatarListSO lits) => avatarListSO = lits;

    public string GetMyUserID() =>  AuthenticationServiceManager.Instance.GetUserId();

    public string GetMyNickname() => AuthenticationServiceManager.Instance.GetPlayerName().Split("#")[0];

    public FriendActivity GetFriendActivity(string friendID) => myFriendDict.ContainsKey(friendID) ? myFriendDict[friendID] : new FriendActivity();

    public FriendActivity GetMyFriendActivity() => myFriendActivity;

    public CharacterData GetMyCharacterData() => myCharacterData;

    private Request LoadFriendOfflineData()
    {
        Request request = new Request(async () =>
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

            if (offlineFriendList.Count == 0) { return; }

            foreach (FriendData friendData in offlineFriendList)
            {
                await CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.FRIEND_ACTIVITY_DATA, friendData.FriendID)
                    .OnSuccessComplete((Item item) => myFriendDict[friendData.FriendID] = item.Value.GetAs<FriendActivity>())
                    .OnFailureComplete(m =>
                    {
                        if (!myFriendDict.ContainsKey(friendData.FriendID))
                        {
                            myFriendDict[friendData.FriendID] = new FriendActivity();
                        }
                    })
                    .StartAndWait();
            }
        });

        return request;
    }

    public void OnFriendDataChangedInvoke() => OnFriendDataChanged?.Invoke();

    private void LobbyServiceManager_OnLobbyGeneralDataUpdate() => OnFriendDataChangedInvoke();

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

                    CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.FRIEND_ACTIVITY_DATA, playerID: friendData.FriendID)
                        .OnSuccessComplete((Item item) => myFriendDict[friendData.FriendID] = item.Value.GetAs<FriendActivity>())
                        .Start();
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

        OnFriendDataChangedInvoke();
    }

    private void FriendServiceManager_OnUserDataUpdate()
    {
        OnUserDataChanged?.Invoke();
    }

    private void FriendServiceManager_OnInitialize()
    {
        FriendServiceManager.Instance.TrySetPresenceActivity(myFriendActivity).Start();

        LoadFriendOfflineData().OnFinalyComplete(OnFriendDataChangedInvoke).Start();
    }

    private void AuthenticationServiceManager_OnSignIn()
    {
        CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.FRIEND_ACTIVITY_DATA, true)
        .OnSuccessComplete((Item item) =>
        {
            myFriendActivity = item.Value.GetAs<FriendActivity>();
            OnUserDataChanged?.Invoke();
        })
        .Start();

        CloudSaveServiceManager.Instance.LoadPlayerData(CloudSaveData.CHARACTER_DATA, false)
            .OnSuccessComplete((Item item) =>
            {
                myCharacterData = item.Value.GetAs<CharacterData>();
                OnUserDataChanged?.Invoke();
            })
            .Start();
    }

    private void AuthenticationServiceManager_OnPlayerInfoChanged()
    {
        LobbyServiceManager.Instance.UpdatePlayerData(LobbyServiceData.PLAYER_NICKNAME_FIELD, AuthenticationServiceManager.Instance.GetPlayerName()).Start();

        OnUserDataChanged?.Invoke();
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
            AuthenticationServiceManager.Instance.OnSignIn -= AuthenticationServiceManager_OnSignIn;
            AuthenticationServiceManager.Instance.OnPlayerInfoChanged -= AuthenticationServiceManager_OnPlayerInfoChanged;
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }

        if (FriendServiceManager.Instance != null)
        {
            FriendServiceManager.Instance.OnInitialize -= FriendServiceManager_OnInitialize;
            FriendServiceManager.Instance.OnUserDataUpdate -= FriendServiceManager_OnUserDataUpdate;
            FriendServiceManager.Instance.OnFriendListChanged -= FriendServiceManager_OnFriendListChanged;
        }

        if (LobbyServiceManager.Instance != null)
        {
            LobbyServiceManager.Instance.OnLobbyGeneralDataUpdate -= LobbyServiceManager_OnLobbyGeneralDataUpdate;
        }
    }
}
