using UnityEngine;
using UnityEngine.UI;
using ServiceRequest;
using TMPro;

public class SetPasswordMenuUI : SystemWindowTrigger
{
    [Header("Reference")]
    [SerializeField] private TMP_InputField oldPasswordField;
    [SerializeField] private TMP_InputField newPasswordField;
    [SerializeField] private Button setButton;

    protected override void Awake()
    {
        base.Awake();

        setButton.onClick.AddListener(SetButton_OnClick);
    }

    private void SetButton_OnClick() => SetNewPassword();

    private void SetNewPassword()
    {
        InformationUI.Instance.Show("Wait...", false);

        RequestResultSettingsExtensions.OnFailureComplete(AuthenticationServiceManager.Instance.TrySetNewPassword(oldPasswordField.text, newPasswordField.text)
            .OnSuccessComplete(() => { InformationUI.Instance.Hide(); window.Close(); })
, m => InformationUI.Instance.Show(m))
            .Start();
    }

    private void OnDisable()
    {
        oldPasswordField.text = string.Empty;
        newPasswordField.text = string.Empty;
    }
}
