using System.Collections.Generic;

public class HealerEffect : CardEffect
{
    private readonly int _healAmount;

    public HealerEffect(int healAmount)
    {
        _healAmount = healAmount;
    }

    public override void Execute(CardInstance attacker, CardInstance primaryTarget, IReadOnlyList<CardInstance> targetField)
    {
        int counterDamage = primaryTarget.currentHP;
        primaryTarget.TakeDamage(attacker.currentHP);
        attacker.TakeDamage(counterDamage);
    }

    public override void OnTurnStart(CardInstance self, IReadOnlyList<CardInstance> allyField)
    {
        foreach (CardInstance card in allyField)
        {
            if (card == null || card == self || !card.IsAlive)
                continue;

            card.Heal(_healAmount);
        }
    }
}
