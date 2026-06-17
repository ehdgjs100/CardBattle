using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [SerializeField] private CardLibrary cardLibrary;

    [Header("Player Collection")]
    [SerializeField] private List<OwnedCardEntry> ownedCards = new List<OwnedCardEntry>();
    [SerializeField] private List<OwnedCardEntry> playerDeck = new List<OwnedCardEntry>();

    [Header("Shop Rates")]
    [SerializeField] private float normalRate = 0.60f;
    [SerializeField] private float specialRate = 0.30f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<CardDataBase> GetBattleDeck()
    {
        var result = new List<CardDataBase>();
        for (int i = 0; i < playerDeck.Count; i++)
        {
            OwnedCardEntry entry = playerDeck[i];
            if (entry?.cardData == null) continue;

            CardDataBase clone = Instantiate(entry.cardData);
            if (entry.upgradeLevel > 0)
                clone.ApplyUpgrade(entry.upgradeLevel);

            result.Add(clone);
        }
        return result;
    }

    public CardDataBase DrawRandom()
    {
        if (cardLibrary == null) return null;

        float roll = Random.value;
        CardRarity rarity;
        if (roll < normalRate)
            rarity = CardRarity.Normal;
        else if (roll < normalRate + specialRate)
            rarity = CardRarity.Special;
        else
            rarity = CardRarity.Epic;

        List<CardDataBase> pool = cardLibrary.GetByRarity(rarity);
        if (pool.Count == 0)
        {
            pool = cardLibrary.allCards;
            if (pool.Count == 0) return null;
        }

        return pool[Random.Range(0, pool.Count)];
    }

    public void AddToCollection(CardDataBase card, int upgradeLevel = 0)
    {
        ownedCards.Add(new OwnedCardEntry { cardData = card, upgradeLevel = upgradeLevel });
    }

    public const int MaxUpgradeLevel = 1;

    public bool UpgradeCard(int ownedIndex)
    {
        if (ownedIndex < 0 || ownedIndex >= ownedCards.Count) return false;
        if (ownedCards[ownedIndex].upgradeLevel >= MaxUpgradeLevel) return false;

        ownedCards[ownedIndex].upgradeLevel++;
        return true;
    }

    public IReadOnlyList<OwnedCardEntry> OwnedCards => ownedCards;
    public IReadOnlyList<OwnedCardEntry> PlayerDeck => playerDeck;
    public CardLibrary Library => cardLibrary;
}
