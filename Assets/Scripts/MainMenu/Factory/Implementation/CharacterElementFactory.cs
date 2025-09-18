using UnityEngine;
using Zenject;

public class CharacterElementFactory : ICharacterElementFactory
{
    [Inject] private CharacterElementUI characterElementUIPrefab;

    public CharacterElementUI CreateCharacterElement(Transform container = null)
    {
        if (characterElementUIPrefab == null) { return null; }

        return Object.Instantiate(characterElementUIPrefab, container);
    }
}
