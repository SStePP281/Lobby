public class LobbyServiceData
{
    /*Data field*/
    public const string DEFAULT_LOBBY_PASSWORD = "default_password";
    public const string READY_FLAG_FIELD = "Ready";
    public const string GAME_ONLINE_TYPE_FIELD = "GameOnlineType";
    public const string GAME_MAP_FIELD = "GameMap";
    public const string PLAYER_NAME_FIELD = "PlayerName";
    public const string PLAYER_CHARACTER_FIELD = "Character";

    /*Lobby settings*/
    public const int maxLobbyCodeLenght = 6;
    public const int minLobbyCodeLenght = 6;

    public const int minClientNameLenght = 6;
    public const int maxClientNameLenght = 15;

    public const int minPasswordLenght = 8;
    public const int maxPasswordLenght = 15;

    public const int maxLobbySize = 12;
}
