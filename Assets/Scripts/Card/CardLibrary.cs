using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardLibrary")]
public class CardLibrary : ScriptableObject
{
    public List<CardDataBase> allCards = new List<CardDataBase>();

    public List<CardDataBase> GetByRarity(CardRarity rarity)
    {
        var result = new List<CardDataBase>();
        for (int i = 0; i < allCards.Count; i++)
        {
            if (allCards[i] != null && allCards[i].rarity == rarity)
                result.Add(allCards[i]);
        }
        return result;
    }
}
