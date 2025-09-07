using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class LobbyCharacterDropdownFieldUI : MonoBehaviour
{
    [SerializeField] private Button kickButton;
    [SerializeField] private Button migrateHostButton;

    private Camera mainCam;
    private RectTransform canvasRect;
    private CanvasGroup canvasGroup;

    private bool isActive = false;
    private string currentClientId;

    private void Awake()
    {
        canvasRect = GetComponentInParent<Canvas>().transform as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();
        mainCam = Camera.main;

        //kickButton.onClick.AddListener(KickButton_OnCliñk);
        //migrateHostButton.onClick.AddListener(MigrateHostButton_OnCliñk);
    }

    private void Start()
    {
        //LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate += LobbyManager_OnLobbyGenerealDataUpdate;

        Hide();
    }

    private void Update()
    {
        if (!isActive) { return; }

        if (Input.GetMouseButtonDown(1))
        {
            //FindClientRaycast();
        }
    }

    //private void LobbyManager_OnLobbyGenerealDataUpdate() => SetEnable(LobbyServiceManager.Instance.IsMyLobby());

    //private void KickButton_OnCliñk()
    //{
    //    if (string.IsNullOrEmpty(currentClientId)) { return; }

    //    LobbyServiceManager.Instance.KickClient(currentClientId);

    //    Hide();
    //}

    //private void MigrateHostButton_OnCliñk()
    //{
    //    if (string.IsNullOrEmpty(currentClientId)) { return; }

    //    LobbyServiceManager.Instance.SetNewLobbyHost(currentClientId);

    //    Hide();
    //}

    //private void FindClientRaycast()
    //{
    //    Vector3 mousePos = Input.mousePosition;
    //    Ray ray = mainCam.ScreenPointToRay(mousePos);

    //    if (Physics.Raycast(ray, out RaycastHit hitInfo))
    //    {
    //        if (hitInfo.collider.TryGetComponent(out LobbyPlayerSetup lobbyPlayerSetup))
    //        {
    //            if (lobbyPlayerSetup.GetClientID() != LobbyServiceManager.Instance.GetMyClientId())
    //            {
    //                currentClientId = lobbyPlayerSetup.GetClientID();

    //                Show(mousePos);

    //                return;
    //            }
    //        }
    //    }

    //    Hide();
    //}

    private void SetEnable(bool isEnable)
    {
        Hide();

        isActive = isEnable;
    }

    private void Show(Vector3 newPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, newPos,
        canvasRect.GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCam,
        out Vector2 localPoint);

        (transform as RectTransform).anchoredPosition = localPoint;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;
    }

    private void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    //private void OnDestroy()
    //{
    //    LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate -= LobbyManager_OnLobbyGenerealDataUpdate;
    //}
}
