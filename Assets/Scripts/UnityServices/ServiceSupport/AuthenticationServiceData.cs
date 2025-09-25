using System.Text.RegularExpressions;

public class AuthenticationServiceData
{
    public const int MIN_PASSWORD_LENGTH = 8;
    public const int MAX_PASSWORD_LENGTH = 30;
    public const int MIN_USER_NAME_LENGTH = 3;
    public const int MAX_USER_NAME_LENGTH = 20;
    public const int MIN_PLAYER_NAME_LENGTH = 6;
    public const int MAX_PLAYER_NAME_LENGTH = 30;

    public static bool CheckValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        if (password.Length < MIN_PASSWORD_LENGTH || password.Length > MAX_PASSWORD_LENGTH) return false;
        if (!Regex.IsMatch(password, "[A-Z]")) return false;
        if (!Regex.IsMatch(password, "[a-z]")) return false;
        if (!Regex.IsMatch(password, "[0-9]")) return false;
        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]")) return false;

        return true;
    }

    public static bool CheckValidUsername(string username)
    {
        return !string.IsNullOrEmpty(username) &&
               username.Length >= MIN_USER_NAME_LENGTH &&
               username.Length <= MAX_USER_NAME_LENGTH;
    }

    public static bool CheckValidPlayerName(string playerName)
    {
        return !string.IsNullOrEmpty(playerName) &&
               playerName.Length >= MIN_PLAYER_NAME_LENGTH &&
               playerName.Length <= MAX_PLAYER_NAME_LENGTH &&
               Regex.IsMatch(playerName, @"^[a-zA-Z0-9_-]+$");
    }

    public static string GetPasswordValidParam() =>
        $"Password must be {MIN_PASSWORD_LENGTH}–{MAX_PASSWORD_LENGTH} characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character.";

    public static string GetUsernameValidParam() =>
        $"Username must be {MIN_USER_NAME_LENGTH}–{MAX_USER_NAME_LENGTH} characters long.";

    public static string GetPlayerNameValidParam() =>
        $"Display name must be {MIN_PLAYER_NAME_LENGTH}–{MAX_PLAYER_NAME_LENGTH} characters long and can only contain letters, numbers, underscores (_), and hyphens (-).";
}