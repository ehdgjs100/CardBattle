using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [SerializeField] private CardLibrary cardLibrary;

    [Header("Shop Rates")]
    [SerializeField] private float normalRate = 0.60f;
    [SerializeField] private float specialRate = 0.30f;

    private readonly List<OwnedCardEntry> ownedCards = new();
    private readonly List<OwnedCardEntry> playerDeck = new();

    private const string KeyGameInit = "GameInitialized";
    private const string KeyOwnedPrefix = "Owned_";
    private const string KeyDeckSize = "DeckSize";
    private const string KeyDeckSlot = "DeckSlot_";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!PlayerPrefs.HasKey(KeyGameInit))
            FirstTimeInit();
        else
            LoadData();
    }

    private void FirstTimeInit()
    {
        if (cardLibrary == null) return;

        List<CardDataBase> normals = cardLibrary.GetByRarity(CardRarity.Normal);
        foreach (CardDataBase card in normals)
        {
            OwnedCardEntry entry = new OwnedCardEntry { cardData = card, upgradeLevel = 0 };
            ownedCards.Add(entry);
            PlayerPrefs.SetInt(KeyOwnedPrefix + card.name, 0);

            if (playerDeck.Count < MaxDeckSize)
                playerDeck.Add(entry);
        }

        SaveDeck();
        PlayerPrefs.SetInt(KeyGameInit, 1);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        LoadCollection();
        LoadDeck();
    }

    private void LoadCollection()
    {
        if (cardLibrary == null) return;

        foreach (CardDataBase card in cardLibrary.allCards)
        {
            string key = KeyOwnedPrefix + card.name;
            if (!PlayerPrefs.HasKey(key)) continue;

            int upgradeLevel = PlayerPrefs.GetInt(key, 0);
            ownedCards.Add(new OwnedCardEntry { cardData = card, upgradeLevel = upgradeLevel });
        }
    }

    private void LoadDeck()
    {
        int size = PlayerPrefs.GetInt(KeyDeckSize, 0);
        for (int i = 0; i < size; i++)
        {
            string cardName = PlayerPrefs.GetString(KeyDeckSlot + i, "");
            if (string.IsNullOrEmpty(cardName)) continue;

            OwnedCardEntry entry = FindOwnedByCardName(cardName);
            if (entry != null)
                playerDeck.Add(entry);
        }
    }

    private OwnedCardEntry FindOwnedByCardName(string cardName)
    {
        for (int i = 0; i < ownedCards.Count; i++)
            if (ownedCards[i].cardData.name == cardName) return ownedCards[i];
        return null;
    }

    public void SaveDeck()
    {
        PlayerPrefs.SetInt(KeyDeckSize, playerDeck.Count);
        for (int i = 0; i < playerDeck.Count; i++)
            PlayerPrefs.SetString(KeyDeckSlot + i, playerDeck[i].cardData.name);
        PlayerPrefs.Save();
    }

    public bool GrantCard(CardDataBase card, int upgradeLevel = 0)
    {
        for (int i = 0; i < ownedCards.Count; i++)
            if (ownedCards[i].cardData == card) return false;

        OwnedCardEntry entry = new OwnedCardEntry { cardData = card, upgradeLevel = upgradeLevel };
        ownedCards.Add(entry);
        PlayerPrefs.SetInt(KeyOwnedPrefix + card.name, upgradeLevel);
        PlayerPrefs.Save();
        return true;
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

    public const int MaxDeckSize = 8;
    public const int MaxUpgradeLevel = 1;

    public bool AddToDeck(int ownedIndex)
    {
        if (playerDeck.Count >= MaxDeckSize) return false;
        if (ownedIndex < 0 || ownedIndex >= ownedCards.Count) return false;

        OwnedCardEntry entry = ownedCards[ownedIndex];
        for (int i = 0; i < playerDeck.Count; i++)
            if (playerDeck[i] == entry) return false;

        playerDeck.Add(entry);
        SaveDeck();
        return true;
    }

    public bool RemoveFromDeck(int deckIndex)
    {
        if (deckIndex < 0 || deckIndex >= playerDeck.Count) return false;
        playerDeck.RemoveAt(deckIndex);
        SaveDeck();
        return true;
    }

    public bool RemoveFromDeckByEntry(OwnedCardEntry entry)
    {
        bool removed = playerDeck.Remove(entry);
        if (removed) SaveDeck();
        return removed;
    }

    public bool UpgradeCard(int ownedIndex)
    {
        if (ownedIndex < 0 || ownedIndex >= ownedCards.Count) return false;
        if (ownedCards[ownedIndex].upgradeLevel >= MaxUpgradeLevel) return false;

        ownedCards[ownedIndex].upgradeLevel++;
        PlayerPrefs.SetInt(KeyOwnedPrefix + ownedCards[ownedIndex].cardData.name, ownedCards[ownedIndex].upgradeLevel);
        PlayerPrefs.Save();
        return true;
    }

    public IReadOnlyList<OwnedCardEntry> OwnedCards => ownedCards;
    public IReadOnlyList<OwnedCardEntry> PlayerDeck => playerDeck;
    public CardLibrary Library => cardLibrary;
}
