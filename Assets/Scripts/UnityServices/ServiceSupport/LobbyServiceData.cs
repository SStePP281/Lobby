public class LobbyServiceData
{
    /*Settings*/
    public const float HEARTBEAT_DELAY = 15.0f;
    public const float UPDATE_LOBBY_DATA_DELAY = 1.1f;
    public const float LOBBY_AUTO_DESTROY_DELAY_IF_ALONE = 12.0f;
    public const int MAX_LOBBY_SIZE = 3;

    /*Lobby setting field*/
    public const string DEFAULT_LOBBY_PASSWORD = "default_password";

    /*User data field*/
    public const string PLAYER_CHARACTER_FIELD = "Character";
    public const string PLAYER_NICKNAME_FIELD = "Nickname";
    public const string PLAYER_READY_FLAG_FIELD = "IsPlayerReady";
}
