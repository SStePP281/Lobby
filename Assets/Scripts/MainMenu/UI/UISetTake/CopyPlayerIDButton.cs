using UnityEngine;
using UnityEngine.UI;

public class CopyPlayerIDButton : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Button copyUserIDButton;

    private void Awake()
    {
        copyUserIDButton.onClick.AddListener(CopyUserIDButton_OnClick);
    }

    private void CopyUserIDButton_OnClick()
    {
        GUIUtility.systemCopyBuffer = HashDataManager.Instance.GetMyUserID();
    }
}
