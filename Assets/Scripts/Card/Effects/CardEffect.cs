using System.Collections.Generic;

public abstract class CardEffect
{
    public abstract void Execute(CardInstance attacker, CardInstance primaryTarget, IReadOnlyList<CardInstance> targetField);

    public virtual void OnTurnStart(CardInstance self, IReadOnlyList<CardInstance> allyField) { }
}
