using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class LobbyCharacterSelectorUI : MonoBehaviour
{
    //[SerializeField] private GameObject baseGridElement;

    //private Dictionary<int, LobbyCharacterGridElementUI> lobbyCharacterElementList;

    //private void Awake()
    //{
    //    lobbyCharacterElementList = new Dictionary<int, LobbyCharacterGridElementUI>();
    //}

    //private void Start()
    //{
    //    Initialize();

    //    LobbyServiceManager.Instance.OnLobbyDataUpdate += LobbyManager_OnLobbyDataUpdate;
    //}

    //private void Initialize()
    //{
    //    int clientCharacter = LobbyCharacterManager.Instance.GetCurrentClientCharacterIndex();

    //    foreach (LobbyPlayerDataSO lobbyPlayer in LobbyCharacterManager.Instance.GetLobbyPlayerDataListSO().characterList)
    //    {
    //        GameObject g = Instantiate(baseGridElement, transform);

    //        if (g.TryGetComponent(out LobbyCharacterGridElementUI elementUI))
    //        {
    //            int characterIndedx = LobbyCharacterManager.Instance.GetLobbyPlayerDataSOFIndex(lobbyPlayer);
    //            elementUI.Initialize(lobbyPlayer, characterIndedx == clientCharacter);
    //            lobbyCharacterElementList[characterIndedx] = elementUI;
    //        }
    //    }
    //}

    //private void LobbyManager_OnLobbyDataUpdate()
    //{
    //    int clientCharacter = LobbyCharacterManager.Instance.GetCurrentClientCharacterIndex();

    //    foreach (var el in lobbyCharacterElementList)
    //    {
    //        el.Value.SetIsSelectedCharacter(el.Key == clientCharacter);
    //    }
    //}

    //private void OnDestroy()
    //{
    //    LobbyServiceManager.Instance.OnLobbyDataUpdate -= LobbyManager_OnLobbyDataUpdate;
    //}
}
