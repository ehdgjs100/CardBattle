using System.Collections.Generic;

public class AssassinEffect : CardEffect
{
    public override bool IsMelee => true;
    public override bool IgnoresTanker => true;
    public override bool IsAssassin => true;

    public override void Execute(CardInstance attacker, CardInstance primaryTarget, IReadOnlyList<CardInstance> targetField)
    {
        primaryTarget.TakeDamage(attacker.currentHP);
    }
}
