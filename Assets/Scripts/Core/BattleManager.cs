using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public event Action<AttackResult, Action> OnAttackPerformed;
    public int TotalKills { get; private set; }

    private readonly Dictionary<CardInstance, int> _otherHPBefore = new();
    private readonly List<SplashHit> _splashHits = new();

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyAttack(CardInstance attacker, CardInstance target, CardField attackerField, CardField targetField, Action onComplete)
    {
        int attackerHPBefore = attacker.currentHP;
        int targetHPBefore = target.currentHP;

        _otherHPBefore.Clear();
        foreach (CardInstance card in targetField.Slots)
        {
            if (card != null && card != target)
                _otherHPBefore[card] = card.currentHP;
        }

        attacker.effect.Execute(attacker, target, targetField.Slots);

        int damageDealt = targetHPBefore - target.currentHP;
        int damageReceived = attackerHPBefore - attacker.currentHP;

        _splashHits.Clear();
        foreach (KeyValuePair<CardInstance, int> entry in _otherHPBefore)
        {
            int splashDamage = entry.Value - entry.Key.currentHP;
            if (splashDamage > 0)
                _splashHits.Add(new SplashHit(entry.Key, splashDamage));
        }

        if (attacker.owner == Owner.Player)
        {
            if (!target.IsAlive) TotalKills++;
            for (int i = 0; i < _splashHits.Count; i++)
                if (!_splashHits[i].Target.IsAlive)
                    TotalKills++;
        }

        void Resolve()
        {
            attackerField.ProcessDeaths();
            targetField.ProcessDeaths();
            onComplete?.Invoke();
        }

        AttackResult result = new AttackResult(attacker, target, damageDealt, damageReceived, _splashHits);

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
