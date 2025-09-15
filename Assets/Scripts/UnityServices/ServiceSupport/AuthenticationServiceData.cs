using System.Text.RegularExpressions;

public class AuthenticationServiceData
{
    public const int MIN_PASSWORD_LENGHT = 8;
    public const int MAX_PASSWORD_LENGHT = 30;
    public const int MIN_USER_NAME_LENGHT = 3;
    public const int MAX_USER_NAME_LENGHT = 20;
    public const int MIN_PLAYER_NAME_LENGHT = 6;
    public const int MAX_PLAYER_NAME_LENGHT = 30;

    public static bool CheckValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        if (password.Length < MIN_PASSWORD_LENGHT || password.Length > MAX_PASSWORD_LENGHT) return false;
        if (!Regex.IsMatch(password, "[A-Z]")) return false;
        if (!Regex.IsMatch(password, "[a-z]")) return false;
        if (!Regex.IsMatch(password, "[0-9]")) return false;
        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]")) return false;

        return true;
    }

    public static bool CheckValidUsername(string username)
    {
        return !string.IsNullOrEmpty(username) && username.Length >= MIN_USER_NAME_LENGHT && username.Length <= MAX_USER_NAME_LENGHT;
    }

    public static bool CheckValidPlayerName(string playerName)
    {
        return !string.IsNullOrEmpty(playerName) && playerName.Length >= MIN_PLAYER_NAME_LENGHT &&
            playerName.Length <= MAX_PLAYER_NAME_LENGHT && Regex.IsMatch(playerName, @"^[a-zA-Z0-9_-]+$");
    }

    public static string GetPasswordValidParam() => $"{MIN_PASSWORD_LENGHT}–{MAX_PASSWORD_LENGHT} chars, 1 upper, 1 lower, 1 number, 1 symbol";

    public static string GetUsernameValidParam() => $"{MIN_USER_NAME_LENGHT}–{MAX_USER_NAME_LENGHT} chars, A-Z, 0-9, . - @ _ (case insensitive)";

    public static string GetPlayerNameValidParam() => $"{MIN_PLAYER_NAME_LENGHT}–{MAX_PLAYER_NAME_LENGHT} chars, no spaces";
}