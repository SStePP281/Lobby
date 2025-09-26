using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationUI : SystemWindowTrigger
{
    public static InformationUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button backButton;

    protected override void Awake()
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

        base.Awake();

        backButton.onClick.AddListener(Hide);
    }

    public void Show(string text, bool showExitButton = true)
    {
        errorText.text = text;

        backButton.gameObject.SetActive(showExitButton);

        window.Open();
    }

    public void Hide() => window.Close();

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
