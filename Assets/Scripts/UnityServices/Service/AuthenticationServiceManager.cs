using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using ServiceRequest;

public class AuthenticationServiceManager : IDisposable
{
    public event Action OnSignIn;
    public event Action OnSignOut;
    public event Action OnPlayerInfoChanged;

    private event Action OnInititialize;

    public bool IsSignedIn { get; private set; }

    private static bool isDisposing = false;
    private static AuthenticationServiceManager instance;
    public static AuthenticationServiceManager Instance
    {
        get
        {
            if (isDisposing) { return null; }

            if (instance == null)
            {
                instance = new AuthenticationServiceManager();
            }

            return instance;
        }
    }

    private AuthenticationServiceManager()
    {
        OnInititialize += AuthenticationServiceManager_OnInitialize;
        OnSignIn += AuthenticationServiceManager_OnSignIn;

        _ = InitializeAsync();
    }

    public Request TrySingIn(string username, string password)
    {
        Request requestResult = new Request(async () =>
        {
            if (IsSignedIn)
            {
                throw new Exception("Already signed in.");
            }

            if (!AuthenticationServiceData.CheckValidUsername(username))
            {
                throw new Exception(AuthenticationServiceData.GetUsernameValidParam());
            }

            if (!AuthenticationServiceData.CheckValidPassword(password))
            {
                throw new Exception(AuthenticationServiceData.GetPasswordValidParam());
            }

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
        });

        requestResult.OnSuccessComplete(() => OnSignIn?.Invoke());

        return requestResult;
    }

    public Request TrySingUp(string username, string passwordOne, string passwordTwo, string playerName)
    {
        Request request = new Request(async () =>
        {
            if (IsSignedIn)
            {
                throw new Exception("Already signed in.");
            }

            if (!AuthenticationServiceData.CheckValidUsername(username))
            {
                throw new Exception(AuthenticationServiceData.GetUsernameValidParam());
            }

            if (!AuthenticationServiceData.CheckValidPassword(passwordOne))
            {
                throw new Exception(AuthenticationServiceData.GetPasswordValidParam());
            }

            if (!AuthenticationServiceData.CheckValidPassword(passwordTwo))
            {
                throw new Exception(AuthenticationServiceData.GetPasswordValidParam());
            }

            if (passwordOne != passwordTwo)
            {
                throw new Exception("Passwords do not match.");
            }

            if (!AuthenticationServiceData.CheckValidPlayerName(playerName))
            {
                throw new Exception(AuthenticationServiceData.GetPlayerNameValidParam());
            }

            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, passwordOne);
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);

        });

        request.OnSuccessComplete(() => OnSignIn?.Invoke());

        return request;
    }

    public Request TrySetNewPlayrName(string newPlayrName)
    {
        Request request = new Request(async () => 
        {
            if (newPlayrName == AuthenticationService.Instance.PlayerName) { return; }
            
            if (!AuthenticationServiceData.CheckValidPlayerName(newPlayrName))
            {
                throw new Exception(AuthenticationServiceData.GetPlayerNameValidParam());
            }

            await AuthenticationService.Instance.UpdatePlayerNameAsync(newPlayrName);
        });

        return request;
    }

    public Request TrySetNewPassword(string oldPassword, string newPassword)
    {
        Request request = new Request(async () => 
        {
            if (!AuthenticationServiceData.CheckValidPassword(oldPassword))
            {
                throw new Exception(AuthenticationServiceData.GetPasswordValidParam());
            }

            if (!AuthenticationServiceData.CheckValidPassword(newPassword))
            {
                throw new Exception(AuthenticationServiceData.GetPasswordValidParam());
            }

            await AuthenticationService.Instance.UpdatePasswordAsync(oldPassword, newPassword);
        });

        return request;
    }

    public void TrySingOut()
    {
        if (!IsSignedIn) { return; }

        OnSignOut?.Invoke();

        AuthenticationService.Instance.SignOut();

        AuthenticationService.Instance.ClearSessionToken();

        IsSignedIn = false;
    }

    public void RefreshUserData() =>  _ = RefreshUserDataAsync();

    public string GetUserId() => AuthenticationService.Instance.PlayerId;

    public string GetPlayerName() => AuthenticationService.Instance.PlayerName;

    private async Task RefreshUserDataAsync()
    {
        try
        {
            await AuthenticationService.Instance.GetPlayerInfoAsync();
            await AuthenticationService.Instance.GetPlayerNameAsync(false);

            Debug.Log($"Update player info; Player name: {AuthenticationService.Instance.PlayerName}, Player ID: {AuthenticationService.Instance.PlayerId}");

            OnPlayerInfoChanged?.Invoke();
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);
        }
        catch (RequestFailedException e)
        {
            Debug.Log(e);
        }
    }

    private async Task InitializeAsync()
    {
        if (!UnityServicesManager.Instance.IsInitialized())
        {
            UnityServicesManager.Instance.OnInitialize += UnityServicesManager_OnInitialize;
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            if (AuthenticationService.Instance.SessionTokenExists)
            {
                await SignInAutoAsync();
            }
        }

        OnInititialize?.Invoke();
    }

    private async Task SignInAutoAsync()
    {
        try
        {
            SignInOptions signInOptions = new SignInOptions { CreateAccount = false };

            await AuthenticationService.Instance.SignInAnonymouslyAsync(signInOptions);

            Debug.Log("Auto sign in");

            OnSignIn?.Invoke();
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);
        }
        catch (RequestFailedException e)
        {
            Debug.Log(e);
        }
    }

    private void AuthenticationService_PlayerNameChanged(string newName) => OnPlayerInfoChanged?.Invoke();

    private void AuthenticationService_PlayerIdChanged(string newId) => OnPlayerInfoChanged?.Invoke();

    private void AuthenticationService_PlayerInfoChanged(PlayerInfo playerInfo) => OnPlayerInfoChanged?.Invoke();

    private void AuthenticationServiceManager_OnInitialize()
    {
        AuthenticationService.Instance.PlayerNameChanged += AuthenticationService_PlayerNameChanged;
        AuthenticationService.Instance.PlayerIdChanged += AuthenticationService_PlayerIdChanged;
        AuthenticationService.Instance.PlayerInfoChanged += AuthenticationService_PlayerInfoChanged;
    }

    private void AuthenticationServiceManager_OnSignIn()
    {
        IsSignedIn = true;

        RefreshUserData();
    }

    private void UnityServicesManager_OnInitialize()
    {
        _ = InitializeAsync();

        UnityServicesManager.Instance.OnInitialize -= UnityServicesManager_OnInitialize;
    }

    public void Dispose()
    {
        OnInititialize -= AuthenticationServiceManager_OnInitialize;
        OnSignIn -= AuthenticationServiceManager_OnSignIn;

        if (UnityServicesManager.Instance != null)
        {
            UnityServicesManager.Instance.OnInitialize -= UnityServicesManager_OnInitialize;
        }

        AuthenticationService.Instance.PlayerNameChanged -= AuthenticationService_PlayerNameChanged;
        AuthenticationService.Instance.PlayerIdChanged -= AuthenticationService_PlayerIdChanged;
        AuthenticationService.Instance.PlayerInfoChanged -= AuthenticationService_PlayerInfoChanged;

        instance = null;

        isDisposing = true;
    }
}
