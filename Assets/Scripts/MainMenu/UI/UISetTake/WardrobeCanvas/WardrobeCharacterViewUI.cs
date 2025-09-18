using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class WardrobeCharacterViewUI : MonoBehaviour, IDragHandler
{
    [Inject] private ICharacterFactory characterFactory;

    [Header("Reference")]
    [SerializeField] private Transform characterSpawnPoint;

    private Transform selectedCharacterModel;

    public void SetNewCharacter(int characterIndex)
    {
        Destroy(selectedCharacterModel);

        selectedCharacterModel = characterFactory.CreateCharacter(characterIndex, characterSpawnPoint);

        //need reset spaw point
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //need rotate spawn point 
        }
    }
}
