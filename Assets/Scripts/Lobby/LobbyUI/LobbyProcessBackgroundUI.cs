using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LobbyProcessBackgroundUI : MonoBehaviour
{
    private const float SHOW_TIME = 2.0f;
    private const float HIDE_TIME = 1.0f;
    public static LobbyProcessBackgroundUI Instance { get; private set; }

    [SerializeField] private Transform waitConnect;
    [SerializeField] private Transform faildConnect;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();

        HideAll();
    }

    public void ShowWaitConnect() => ShowAny(waitConnect, false);

    public void ShowFaildConnect() => ShowAny(faildConnect, true);

    public void HideAll(bool setTime = false)
    {
        if (setTime)
        {
            StartCoroutine(HideAnyCor(transform));
        }
        else
        {
            gameObject.SetActive(false);
            waitConnect.gameObject.SetActive(false);
            faildConnect.gameObject.SetActive(false);

            canvasGroup.alpha = 1.0f;
        }
    }

    private void ShowAny(Transform t, bool setTimer)
    {
        HideAll();
        gameObject.SetActive(true);
        t.gameObject.SetActive(true);

        if (setTimer)
        {
            StartCoroutine(ShowAnyCor(t));
        }
    }

    private IEnumerator HideAnyCor(Transform t)
    {
        float progres = HIDE_TIME;

        while (progres >= 0)
        {
            canvasGroup.alpha = progres / HIDE_TIME;

            yield return null;

            progres -= Time.deltaTime;
        }

        t.gameObject.SetActive(false);
        gameObject.SetActive(false);
        canvasGroup.alpha = 1.0f;
    }

    private IEnumerator ShowAnyCor(Transform t)
    {
        yield return new WaitForSeconds(SHOW_TIME);

        HideAll(true);
    }
}
