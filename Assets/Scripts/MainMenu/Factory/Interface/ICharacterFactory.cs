using UnityEngine;

public interface ICharacterFactory
{
    public Transform CreateCharacter(int characterIndex, Transform spawnPoint = null);
}
