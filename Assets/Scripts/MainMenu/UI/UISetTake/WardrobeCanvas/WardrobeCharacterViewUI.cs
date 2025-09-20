using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class WardrobeCharacterViewUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Inject] private ICharacterFactory characterFactory;

    [Header("Reference")]
    [SerializeField] private Transform characterSpawnPoint;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float accelearion = 5.0f;

    private Transform selectedCharacterModel;
    private Vector3 startRotation;

    private float currentVelocity;

    private void Awake()
    {
        startRotation = characterSpawnPoint.rotation.eulerAngles;
    }

    public void SetNewCharacter(int characterIndex)
    {
        if (selectedCharacterModel != null)
        {
            Destroy(selectedCharacterModel.gameObject);
        }

        selectedCharacterModel = characterFactory.CreateCharacter(characterIndex, characterSpawnPoint);
        characterSpawnPoint.rotation = Quaternion.Euler(startRotation);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        DOTween.Kill(characterSpawnPoint);
        currentVelocity = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        float delta = -eventData.delta.x * rotationSpeed;
        characterSpawnPoint.Rotate(Vector3.up, delta);

        currentVelocity = delta * accelearion;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Mathf.Abs(currentVelocity) <= 1.0f) { return; }

            characterSpawnPoint.DORotate(characterSpawnPoint.rotation.eulerAngles + Vector3.up * currentVelocity, 1.0f)
                               .SetEase(Ease.OutQuad)
                               .SetUpdate(true);
        }
    }
}