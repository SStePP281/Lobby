using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CanvasGroup))]
public class NotificationElementUI : BaseUIListElement<FriendData>
{
    [Header("Reference")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private List<TextMeshProUGUI> textList;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup canvasGroup;
    private Sequence showSequence;
    private Action onDestroyedCallback;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        showSequence = DOTween.Sequence()
            .Append(canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.Linear))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                onDestroyedCallback?.Invoke();
                onDestroyedCallback = null;
            })
            .SetAutoKill(false)
            .Pause();
    }

    public NotificationElementUI Initialize(FriendData userData, string message)
    {
        string[] words = message.Split(" ");

        for (int i = 0; i < textList.Count; i++)
        {
            textList[i].text = words[i];
        }

        transform.SetAsLastSibling();

        Initialize(userData);

        return this;
    }

    public override BaseUIListElement<FriendData> Initialize(FriendData userData)
    {
        base.Initialize(userData);

        nicknameText.text = userData.FriendName;
        avatarImage.sprite = userData.Avatar;

        canvasGroup.alpha = 1f;

        return this;
    }

    public NotificationElementUI SetOnDestroyed(Action callback)
    {
        onDestroyedCallback = callback;
        return this;
    }

    public void StartDestroyTimer(float showTime) => showSequence.SetDelay(showTime).Restart(true, showTime);

    public void RestartDestroyTimer(float showTime)
    {
        canvasGroup.alpha = 1f;
        transform.SetAsLastSibling();

        StartDestroyTimer(showTime);
    }

    private void OnDestroy()
    {
        showSequence?.Kill();
        showSequence = null;
    }
}
