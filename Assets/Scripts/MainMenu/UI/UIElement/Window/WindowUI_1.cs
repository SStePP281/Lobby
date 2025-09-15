using System;
using DG.Tweening;
using UnityEngine;

public class WindowUI_1 : MonoBehaviour, IBaseWindowUI
{
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private bool closeOnStart = true;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        gameObject.SetActive(!closeOnStart);
    }

    public void Open()
    {
        if (gameObject.activeSelf) { return; }

        Action callback = () =>
        {
            transform.localScale = originalScale;
            gameObject.SetActive(true);
        };

        OnOpenAnim(callback);
    }

    public void Close()
    {
        if (!gameObject.activeSelf) { return; }

        Action callback = () =>
        {
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        };

        OnCloseAnim(callback);
    }

    private void OnOpenAnim(Action callback = null)
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;

        transform.DOKill();

        transform.DOScale(originalScale, animDuration)
            .SetEase(openEase)
            .OnComplete(() => callback?.Invoke());
    }

    private void OnCloseAnim(Action callback = null)
    {
        gameObject.SetActive(true);
        transform.localScale = originalScale;

        transform.DOKill();

        transform.DOScale(Vector3.zero, animDuration)
            .SetEase(closeEase)
            .OnComplete(() => callback?.Invoke());
    }
}