using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationUI : MonoBehaviour
{
    public static InformationUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button backButton;

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

        backButton.onClick.AddListener(Hide);

        Hide();
    }

    public void Show(string text, bool showExitButton = true)
    {
        errorText.text = text;

        backButton.gameObject.SetActive(showExitButton);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
