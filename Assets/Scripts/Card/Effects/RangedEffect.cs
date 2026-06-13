using System.Collections.Generic;

public class RangedEffect : CardEffect
{
    public override void Execute(CardInstance attacker, CardInstance primaryTarget, IReadOnlyList<CardInstance> targetField)
    {
        primaryTarget.TakeDamage(attacker.currentHP);
    }
}
