using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        if (AuthenticationServiceManager.Instance.TrySetNewPassword(oldPasswordField.text, newPasswordField.text,          
                                                  AuthenticationServiceManager_TrySetNewPassword_Callback))
        {
            InformationUI.Instance.Show("Wait...", false);
        }
        else
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPasswordValidParam());
        }
    }

    private void AuthenticationServiceManager_TrySetNewPassword_Callback(bool succes, string errorMessage)
    {
        if (succes)
        {
            InformationUI.Instance.Hide();

            window.Close();
        }
        else
        {
            InformationUI.Instance.Show(errorMessage);
        }
    }

    private void OnDisable()
    {
        oldPasswordField.text = string.Empty;
        newPasswordField.text = string.Empty;
    }
}
