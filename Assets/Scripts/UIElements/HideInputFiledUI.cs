using TMPro;
using UnityEngine;

public class HideInputFiledUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    public void HidePassword(bool isActive)
    {
        inputField.contentType = isActive ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        inputField.ForceLabelUpdate();
    }
}
