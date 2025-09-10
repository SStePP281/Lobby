using UnityEngine;

public abstract class SystemWindowTrigger : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject baseWindowUI;

    protected IBaseWindowUI window;

    protected virtual void Awake()
    {
        window = baseWindowUI.GetComponent<IBaseWindowUI>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (baseWindowUI != null)
        {
            if (baseWindowUI.GetComponent<IBaseWindowUI>() == null)
            {
                baseWindowUI = null;
            }
        }
    }
#endif
}
