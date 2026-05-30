using UnityEngine;

[CreateAssetMenu(fileName = "New Card Data", menuName = "Puzzle/Memory/Card Data")]
public class CardData : ScriptableObject
{
    public int cardID;
    public Sprite cardSprite;
    public Sprite fixedSprite;
}
