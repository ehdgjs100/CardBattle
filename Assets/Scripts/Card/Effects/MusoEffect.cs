using System.Collections.Generic;
using UnityEngine;

public class MusoEffect : CardEffect
{
    private readonly float _splashRatio;

    public MusoEffect(float splashRatio)
    {
        _splashRatio = splashRatio;
    }

    public override void Execute(CardInstance attacker, CardInstance primaryTarget, IReadOnlyList<CardInstance> targetField)
    {
        primaryTarget.TakeDamage(attacker.currentHP);

        List<CardInstance> adjacent = new List<CardInstance>();
        foreach (CardInstance card in targetField)
        {
            if (card == null || card == primaryTarget || !card.IsAlive)
                continue;

            if (Mathf.Abs(card.slotIndex - primaryTarget.slotIndex) == 1)
                adjacent.Add(card);
        }

        if (adjacent.Count == 0)
            return;

        CardInstance splashTarget = adjacent[Random.Range(0, adjacent.Count)];
        int splashDamage = Mathf.RoundToInt(attacker.currentHP * _splashRatio);
        splashTarget.TakeDamage(splashDamage);
    }
}
