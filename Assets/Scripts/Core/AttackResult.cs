using System.Collections.Generic;

public class AttackResult
{
    public CardInstance Attacker { get; }
    public CardInstance Target { get; }
    public int DamageDealt { get; }
    public int DamageReceived { get; }
    public IReadOnlyList<SplashHit> SplashHits { get; }

    public AttackResult(CardInstance attacker, CardInstance target, int damageDealt, int damageReceived, IReadOnlyList<SplashHit> splashHits)
    {
        Attacker = attacker;
        Target = target;
        DamageDealt = damageDealt;
        DamageReceived = damageReceived;
        SplashHits = splashHits;
    }
}

public readonly struct SplashHit
{
    public readonly CardInstance Target;
    public readonly int Damage;

    public SplashHit(CardInstance target, int damage)
    {
        Target = target;
        Damage = damage;
    }
}
