using System.Collections.Generic;

public abstract class CardEffect
{
    public virtual bool IsMelee => false;
    public virtual bool DealsCounterDamage => true;

    public abstract void Execute(CardInstance attacker, CardInstance primaryTarget, IReadOnlyList<CardInstance> targetField);

    public virtual void OnTurnStart(CardInstance self, IReadOnlyList<CardInstance> allyField) { }
}
