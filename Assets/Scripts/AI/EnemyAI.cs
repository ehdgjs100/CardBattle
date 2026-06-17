public static class EnemyAI
{
    public static bool TryDecideAction(CardField enemyField, CardField playerField, out CardInstance attacker, out CardInstance target)
    {
        attacker = null;
        target = null;

        int bestScore = int.MinValue;

        for (int i = 0; i < CardField.SlotCount; i++)
        {
            CardInstance candidate = enemyField.Slots[i];
            if (candidate == null || !candidate.IsAlive || candidate.data.CardType == CardType.Tanker)
                continue;

            CardInstance bestTarget = FindBestTarget(candidate, playerField);
            if (bestTarget == null) continue;

            int score = ScoreTarget(candidate, bestTarget) + GetEffectivePower(candidate);
            if (score > bestScore)
            {
                bestScore = score;
                attacker = candidate;
                target = bestTarget;
            }
        }

        return target != null;
    }

    private static CardInstance FindBestTarget(CardInstance attacker, CardField playerField)
    {
        CardInstance bestTarget = null;
        int bestScore = int.MinValue;

        bool mustHitTanker = playerField.HasActiveTanker() && !attacker.effect.IgnoresTanker;

        for (int i = 0; i < CardField.SlotCount; i++)
        {
            CardInstance card = playerField.Slots[i];
            if (card == null || !card.IsAlive) continue;
            if (mustHitTanker && card.data.CardType != CardType.Tanker) continue;

            int score = ScoreTarget(attacker, card);
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = card;
            }
        }

        return bestTarget;
    }

    private static int ScoreTarget(CardInstance attacker, CardInstance target)
    {
        bool canKill = GetEffectivePower(attacker) >= target.currentHP;
        int threat = GetThreatScore(target.data.CardType);
        int damageProgress = target.data.maxHP - target.currentHP;
        return threat * 1000 + (canKill ? 2000 : 0) + damageProgress;
    }

    private static int GetEffectivePower(CardInstance card)
    {
        float multiplier = card.data.CardType switch
        {
            CardType.Muso     => 1.5f,
            CardType.Assassin => 1.3f,
            _                 => 1.0f
        };
        return (int)(card.currentHP * multiplier);
    }

    private static int GetThreatScore(CardType type)
    {
        switch (type)
        {
            case CardType.Healer:   return 5;
            case CardType.Ranged:   return 4;
            case CardType.Muso:     return 3;
            case CardType.Assassin: return 2;
            case CardType.Normal:   return 1;
            default:                return 0;
        }
    }
}
