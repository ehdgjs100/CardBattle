using System.Collections.Generic;

public class HealerEffect : CardEffect
{
    private readonly int _healAmount;

    public override bool IsMelee => true;

    public HealerEffect(int healAmount)
    {
        _healAmount = healAmount;
    }

    public override void Execute(CardInstance attacker, CardInstance primaryTarget, IReadOnlyList<CardInstance> targetField)
    {
        int counterDamage = primaryTarget.currentHP;
        primaryTarget.TakeDamage(attacker.currentHP);
        if (primaryTarget.effect.DealsCounterDamage)
            attacker.TakeDamage(counterDamage);
    }

    public override void OnTurnStart(CardInstance self, IReadOnlyList<CardInstance> allyField)
    {
        bool healed = false;
        foreach (CardInstance card in allyField)
        {
            if (card == null || !card.IsAlive)
                continue;

            card.QueueHeal(_healAmount);
            healed = true;
        }

        if (healed)
            self.RaiseHealCast();
    }
}
