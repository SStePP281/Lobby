using UnityEngine;
using UnityEngine.UI;
using ServiceRequest;
using TMPro;

public class SetNameMenuUI : SystemWindowTrigger
{
    [Header("Reference")]
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Button setNameButton;

    protected override void Awake()
    {
        base.Awake();

        setNameButton.onClick.AddListener(SetNameButton_OnClick);
    }

    private void SetNameButton_OnClick() => SetNewName();

    private void SetNewName()
    {
        InformationUI.Instance.Show("Wait...", false);

        AuthenticationServiceManager.Instance.TrySetNewPlayrName(nameField.text)
                                             .OnSuccessComplete(() => { InformationUI.Instance.Hide(); window.Close(); })
                                             .OnFailureComplete(m => InformationUI.Instance.Show(m))
                                             .Start();
    }

    private void OnDisable()
    {
        nameField.text = string.Empty;
    }
}
