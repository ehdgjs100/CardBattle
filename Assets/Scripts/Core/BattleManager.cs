using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public event Action<CardInstance, CardInstance, Action> OnAttackPerformed;

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyAttack(CardInstance attacker, CardInstance target, CardField attackerField, CardField targetField, Action onComplete)
    {
        int attackerHPBefore = attacker.currentHP;
        int targetHPBefore = target.currentHP;

        attacker.effect.Execute(attacker, target, targetField.Slots);

        int damageDealt = targetHPBefore - target.currentHP;
        int damageReceived = attackerHPBefore - attacker.currentHP;
        Debug.Log($"[Battle] {attacker.owner} {attacker.data.name}({attackerHPBefore}->{attacker.currentHP}) attacks " +
            $"{target.owner} {target.data.name}({targetHPBefore}->{target.currentHP}) : dealt {damageDealt}, received {damageReceived}");

        void Resolve()
        {
            attackerField.ProcessDeaths();
            targetField.ProcessDeaths();
            onComplete?.Invoke();
        }

        if (OnAttackPerformed != null)
            OnAttackPerformed.Invoke(attacker, target, Resolve);
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
