using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public event Action<AttackResult, Action> OnAttackPerformed;
    public int TotalKills { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyAttack(CardInstance attacker, CardInstance target, CardField attackerField, CardField targetField, Action onComplete)
    {
        int attackerHPBefore = attacker.currentHP;
        int targetHPBefore = target.currentHP;

        Dictionary<CardInstance, int> otherHPBefore = new Dictionary<CardInstance, int>();
        foreach (CardInstance card in targetField.Slots)
        {
            if (card != null && card != target)
                otherHPBefore[card] = card.currentHP;
        }

        attacker.effect.Execute(attacker, target, targetField.Slots);

        int damageDealt = targetHPBefore - target.currentHP;
        int damageReceived = attackerHPBefore - attacker.currentHP;

        List<SplashHit> splashHits = new List<SplashHit>();
        foreach (KeyValuePair<CardInstance, int> entry in otherHPBefore)
        {
            int splashDamage = entry.Value - entry.Key.currentHP;
            if (splashDamage > 0)
                splashHits.Add(new SplashHit(entry.Key, splashDamage));
        }

        if (attacker.owner == Owner.Player)
        {
            if (!target.IsAlive) TotalKills++;
            for (int i = 0; i < splashHits.Count; i++)
                if (!splashHits[i].Target.IsAlive)
                    TotalKills++;
        }

        void Resolve()
        {
            attackerField.ProcessDeaths();
            targetField.ProcessDeaths();
            onComplete?.Invoke();
        }

        AttackResult result = new AttackResult(attacker, target, damageDealt, damageReceived, splashHits);

        if (OnAttackPerformed != null)
            OnAttackPerformed.Invoke(result, Resolve);
        else
            Resolve();
    }

    public GameResult CheckResult(CardField playerField, CardField enemyField)
    {
        if (enemyField.IsDefeated())
            return GameResult.Win;

        if (playerField.IsDefeated())
            return GameResult.Lose;

        return GameResult.None;
    }
}
