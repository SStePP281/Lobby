using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SwitchedButtonUI : MonoBehaviour
{
    public Button Button { get; private set; }

    [SerializeField] private Transform active;
    [SerializeField] private Transform disactive;

    [SerializeField] private bool isActive = true;

    public UnityEvent<bool> OnChangeActive;

    private void Awake()
    {
        Button = GetComponent<Button>();

        Button.onClick.AddListener(Change);
    }

    private void Start()
    {
        isActive = !isActive;
        Change();
    }

    private void Change()
    {
        isActive = !isActive;

        active.gameObject.SetActive(isActive);
        disactive.gameObject.SetActive(!isActive);

        OnChangeActive?.Invoke(isActive);
    }

    public void SetState(bool isActive)
    {
        if (this.isActive == isActive) { return; }

        this.isActive = isActive;

        active.gameObject.SetActive(isActive);
        disactive.gameObject.SetActive(!isActive);
    }
}
