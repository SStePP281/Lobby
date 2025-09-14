using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationServiceManager : IDisposable
{
    public event Action OnSignIn;
    public event Action OnSignOut;
    public event Action OnPlayerInfoChanged;
    public event Action<string> OnSignInFailed;
    public event Action<string> OnSingUpFailed;

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
                await SignInAuto();
            }
        }

        OnInititialize?.Invoke();
    }

    public bool TrySingIn(string username, string password, Action<bool,string> callback = null)
    {
        if (IsSignedIn) { return true; }

        if (CheckValidUsername(username) && CheckValidPassword(password))
        {
            _ = SingInAsync(username, password, callback);

            return true;
        }

        return false;
    }

    public bool TrySingUp(string username, string password, string playerName, Action<bool, string> callback = null)
    {
        if (IsSignedIn) { return true; }

        if (CheckValidUsername(username) && CheckValidPassword(password) && CheckValidPlayerName(playerName))
        {
            _ = SingUpAsync(username, password, playerName, callback);

            return true;
        }

        return false;
    }

    public bool TrySetNewPlayrName(string newPlayrName, Action<bool, string> callback = null)
    {
        if (newPlayrName == AuthenticationService.Instance.PlayerName) { return true; }

        if (CheckValidPlayerName(newPlayrName))
        {
            _ = UpdatePlayerNameAsync(newPlayrName, callback);

            return true;
        }

        return false;
    }

    public bool TrySetNewPassword(string oldPassword, string newPassword, Action<bool, string> callback = null)
    {
        if (CheckValidPassword(oldPassword) && CheckValidPassword(newPassword))
        {
            if (oldPassword != newPassword)
            {
                _ = UpdatePasswordAsync(oldPassword, newPassword, callback);

                return true;
            }
        }

        return false;
    }

    public void TrySingOut()
    {
        if (!IsSignedIn) { return; }

        OnSignOut?.Invoke();

        AuthenticationService.Instance.SignOut();

        AuthenticationService.Instance.ClearSessionToken();

        IsSignedIn = false;
    }

    public void RefreshUserData()
    {
        _ = RefreshUserDataAsync();   
    }

    private async Task SignInAuto()
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

            OnSignInFailed?.Invoke(e.Message);
        }
        catch (RequestFailedException e)
        {
            Debug.Log(e);

            OnSignInFailed?.Invoke(e.Message);
        }
    }

    private async Task SingInAsync(string username, string password, Action<bool, string> callback = null)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

            Debug.Log("Succes enter in account");

            callback?.Invoke(true, string.Empty);
            OnSignIn?.Invoke();
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
            OnSignInFailed?.Invoke(e.Message);
        }
        catch (RequestFailedException e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
            OnSignInFailed?.Invoke(e.Message);
        }
        catch (Exception e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
            OnSignInFailed?.Invoke(e.Message);
        }
    }

    private async Task SingUpAsync(string username, string password, string playerName, Action<bool, string> callback = null)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            await UpdatePlayerNameAsync(playerName);

            Debug.Log($"Create new account: Username: {username}, Password: {password}");

            callback?.Invoke(true, string.Empty);
            OnSignIn?.Invoke();
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
            OnSingUpFailed?.Invoke(e.Message);
        }
        catch (RequestFailedException e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
            OnSingUpFailed?.Invoke(e.Message);
        }
        catch (Exception e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
            OnSingUpFailed?.Invoke(e.Message);
        }
    }

    private async Task UpdatePasswordAsync(string oldPassword, string newPassword, Action<bool, string> callback = null)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePasswordAsync(oldPassword, newPassword);

            Debug.Log($"Set new password: {newPassword}");

            callback?.Invoke(true, string.Empty);
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
        }
        catch (RequestFailedException e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
        }
        catch (Exception e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
        }
    }

    private async Task UpdatePlayerNameAsync(string newPlayerName, Action<bool, string> callback = null)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newPlayerName);

            Debug.Log($"Set new player name: {newPlayerName}");

            callback?.Invoke(true, string.Empty);
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
        }
        catch (RequestFailedException e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
        }
        catch (Exception e)
        {
            Debug.Log(e);

            callback?.Invoke(false, e.Message);
        }
    }

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

    public bool CheckValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        if (password.Length < 8 || password.Length > 30) return false;
        if (!Regex.IsMatch(password, "[A-Z]")) return false;
        if (!Regex.IsMatch(password, "[a-z]")) return false;
        if (!Regex.IsMatch(password, "[0-9]")) return false;
        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]")) return false;

        return true;
    }

    public bool CheckValidUsername(string username)
    {
        return !string.IsNullOrEmpty(username) && username.Length >= AuthenticationServiceData.MIN_USER_NAME_LENGHT && username.Length <= AuthenticationServiceData.MAX_USER_NAME_LENGHT;
    }

    public bool CheckValidPlayerName(string playerName)
    {
        return !string.IsNullOrEmpty(playerName) && playerName.Length >= AuthenticationServiceData.MIN_PLAYER_NAME_LENGHT && 
            playerName.Length <= AuthenticationServiceData.MAX_PLAYER_NAME_LENGHT && Regex.IsMatch(playerName, @"^[a-zA-Z0-9_-]+$");
    }

    public string GetUserId() => AuthenticationService.Instance.PlayerId;

    public string GetPlayerName() => AuthenticationService.Instance.PlayerName;

    public string GetPasswordValidParam()
    {
        return $"{AuthenticationServiceData.MIN_PASSWORD_LENGHT}–{AuthenticationServiceData.MAX_PASSWORD_LENGHT} chars, 1 upper, 1 lower, 1 number, 1 symbol";
    }

    public string GetUsernameValidParam()
    {
        return $"{AuthenticationServiceData.MIN_USER_NAME_LENGHT}–{AuthenticationServiceData.MAX_USER_NAME_LENGHT} chars, A-Z, 0-9, . - @ _ (case insensitive)";
    }

    public string GetPlayerNameValidParam()
    {
        return $"{AuthenticationServiceData.MIN_PLAYER_NAME_LENGHT}–{AuthenticationServiceData.MAX_PLAYER_NAME_LENGHT} chars, no spaces";
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
