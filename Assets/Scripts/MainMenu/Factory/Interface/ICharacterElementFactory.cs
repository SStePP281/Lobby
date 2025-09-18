using UnityEngine;

public interface ICharacterElementFactory
{
    public CharacterElementUI CreateCharacterElement(Transform container = null);
}
