using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public struct LobbyPlayerData
{
    public LobbyPlayerData(string clientID, string clientName, LobbyPlayerDataSO lobbyPlayerDataSO)
    {
        this.clientID = clientID;
        this.clientName = clientName;
        this.lobbyPlayerDataSO = lobbyPlayerDataSO;
    }

    public string clientName;
    public string clientID;
    public LobbyPlayerDataSO lobbyPlayerDataSO;
}

public class LobbyPlayerSetup : MonoBehaviour
{
    [SerializeField] private Transform modelTransfrom;
    [SerializeField] private Image hostImage;
    [SerializeField] private Image readyImage;
    [SerializeField] private TextMeshProUGUI nameText;

    private LobbyPlayerData playerData;

    private void Awake()
    {
        playerData = new LobbyPlayerData();
    }
    public void UpdatePlalaerData(Player player)
    {
        SetNewClientID(player.Id);
        SetNewClientName(player.Data[LobbyData.PLAYER_NAME_FIELD].Value);
        SetNewLobbyPlayerDataSO(LobbyCharacterManager.Instance.GetLobbyPlayerDataSOFromIndex(int.Parse(player.Data[LobbyData.PLAYER_CHARACTER_FIELD].Value)));
        SetNewReadyFlagState(bool.Parse(player.Data[LobbyData.READY_FLAG_FIELD].Value));
    }

    public void UpdatePlayerCharacterData(LobbyPlayerDataSO playerSO)
    {
        SetNewLobbyPlayerDataSO(playerSO);
    }

    public void SetHostFlag(bool isHost)
    {
        hostImage.gameObject.SetActive(isHost);
    }

    private void SetNewClientName(string newClientName)
    {
        if (newClientName == playerData.clientName) { return; }

        playerData.clientName = newClientName;
        nameText.text = newClientName;
    }

    private void SetNewClientID(string clientID)
    {
        if (playerData.clientID == clientID) { return; }

        playerData.clientID = clientID;
    }

    private void SetNewLobbyPlayerDataSO(LobbyPlayerDataSO newPlayerDataSO)
    {
        if (newPlayerDataSO == null)
        {
            Debug.Log("New Player Data SO is null!");
            return;
        }

        if (playerData.lobbyPlayerDataSO != null && newPlayerDataSO.characterName == playerData.lobbyPlayerDataSO.characterName)
        {
            return;
        }

        playerData.lobbyPlayerDataSO = newPlayerDataSO;

        if (modelTransfrom.childCount > 0)
        {
            for (int i = 0; i < modelTransfrom.childCount; i++)
            {
                Destroy(modelTransfrom.GetChild(i).gameObject);
            }
        }

        Transform newModel = Instantiate(newPlayerDataSO.characterPrefab, modelTransfrom);
        newModel.localPosition = Vector3.zero;
        newModel.localRotation = Quaternion.identity;
        newModel.localScale = Vector3.one;
    }

    private void SetNewReadyFlagState(bool isReady)
    {
        readyImage.gameObject.SetActive(isReady);
    }

    public LobbyPlayerDataSO GetLobbyPlayerDataSO() => playerData.lobbyPlayerDataSO;

    public string GetClientID() => playerData.clientID;
}