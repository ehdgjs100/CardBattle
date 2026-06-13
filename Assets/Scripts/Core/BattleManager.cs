using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyAttack(CardInstance attacker, CardInstance target, CardField attackerField, CardField targetField)
    {
        attacker.effect.Execute(attacker, target, targetField.Slots);
        attackerField.ProcessDeaths();
        targetField.ProcessDeaths();
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
