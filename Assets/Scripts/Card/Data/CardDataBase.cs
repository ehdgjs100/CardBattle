using UnityEngine;

public abstract class CardDataBase : ScriptableObject
{
    public string cardName;
    public int maxHP;
    public string feature;
    public string description;
    public CardVisualConfig visual;

    public abstract CardType CardType { get; }
    public abstract CardEffect CreateEffect();
}
