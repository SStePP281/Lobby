using System;

[Serializable]
public class CharacterData
{
    public CharacterData() : this(0) { }

    public CharacterData(int characterIndex)
    {
        CharacterIndex = characterIndex;
    }

    public int CharacterIndex { get; set; }
}