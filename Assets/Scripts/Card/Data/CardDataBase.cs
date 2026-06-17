using UnityEngine;

public abstract class CardDataBase : ScriptableObject
{
    public string cardName;
    public int maxHP;
    public string feature;
    public string description;
    public CardVisualConfig visual;
    public CardRarity rarity = CardRarity.Normal;

    [Header("Upgrade")]
    public int hpPerUpgrade = 2;

    public int UpgradeLevel { get; private set; }

    public abstract CardType CardType { get; }
    public abstract CardEffect CreateEffect();

    public UnityEngine.Color GetRarityColor()
    {
        switch (rarity)
        {
            case CardRarity.Special: return new UnityEngine.Color(0.2f, 0.85f, 0.2f);
            case CardRarity.Epic:    return new UnityEngine.Color(0.9f, 0.15f, 0.15f);
            default:                 return UnityEngine.Color.white;
        }
    }

    public virtual void ApplyUpgrade(int level)
    {
        UpgradeLevel = level;
        maxHP += hpPerUpgrade * level;
    }
}
