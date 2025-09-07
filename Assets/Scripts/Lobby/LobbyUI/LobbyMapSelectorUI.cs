using UnityEngine;
using UnityEngine.UI;

public class LobbyMapSelectorUI : MonoBehaviour
{
    [Header("Spring")]
    [SerializeField] private BaseSpringTrigger springTrigger;

    [Header("Set")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject mapGridElementPrefab;

    private void Awake()
    {
        openButton.onClick.AddListener(Show);
        openButton.onClick.AddListener(() => { Debug.Log("Open button click"); });
        backButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        Initialize();

        Hide();
    }

    private void Initialize()
    {
        foreach (LobbyMapDataSO map in LobbyGameManager.Instance.GetLobbyMapDataList())
        {
            GameObject g = Instantiate(mapGridElementPrefab, gridParent);

            if (g.TryGetComponent(out LobbyMapGridElementUI elementUI))
            {
                elementUI.Initialize(map);
            }
        }
    }

    private void Show()
    {
        Debug.Log("Set open button new spring state");
        springTrigger.SetNewSpringState(SpringState.Normal);
    }

    private void Hide()
    {
        springTrigger.SetNewSpringState(SpringState.Negative);
    }
}
