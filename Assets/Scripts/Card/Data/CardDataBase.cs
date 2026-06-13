using UnityEngine;

public abstract class CardDataBase : ScriptableObject
{
    public string cardName;
    public int maxHP;
    public string description;
    public CardVisualConfig visual;

    public abstract CardEffect CreateEffect();
}
