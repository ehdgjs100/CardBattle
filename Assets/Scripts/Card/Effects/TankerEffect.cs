using System.Collections.Generic;

public class TankerEffect : CardEffect
{
    public override bool IsMelee => true;
    public override bool DealsCounterDamage => false;

    public override void Execute(CardInstance attacker, CardInstance primaryTarget, IReadOnlyList<CardInstance> targetField)
    {
        int counterDamage = primaryTarget.currentHP;
        primaryTarget.TakeDamage(attacker.currentHP);
        attacker.TakeDamage(counterDamage);
    }
}
