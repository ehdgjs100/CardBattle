using System.Collections.Generic;

public class NormalEffect : CardEffect
{
    public override bool IsMelee => true;

    public override void Execute(CardInstance attacker, CardInstance primaryTarget, IReadOnlyList<CardInstance> targetField)
    {
        int counterDamage = primaryTarget.currentHP;
        primaryTarget.TakeDamage(attacker.currentHP);
        attacker.TakeDamage(counterDamage);
    }
}
