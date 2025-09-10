using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        if (AuthenticationServiceManager.Instance.TrySetNewPlayrName(nameField.text, AuthenticationServiceManager_TrySetNewPlayrName_Callback))
        {
            InformationUI.Instance.Show("Wait..." , false);
        }
        else
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPlayerNameValidParam());
        }
    }

    private void AuthenticationServiceManager_TrySetNewPlayrName_Callback(bool succes, string errorMessage)
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
        nameField.text = string.Empty;
    }
}
