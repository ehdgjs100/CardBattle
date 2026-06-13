public static class EnemyAI
{
    public static bool TryDecideAction(CardField enemyField, CardField playerField, out CardInstance attacker, out CardInstance target)
    {
        attacker = null;
        target = null;

        for (int i = 0; i < CardField.SlotCount; i++)
        {
            CardInstance card = enemyField.Slots[i];
            if (card == null || !card.IsAlive)
                continue;

            if (attacker == null || card.currentHP > attacker.currentHP)
                attacker = card;
        }

        if (attacker == null)
            return false;

        for (int i = 0; i < CardField.SlotCount; i++)
        {
            CardInstance card = playerField.Slots[i];
            if (card == null || !card.IsAlive)
                continue;

            if (target == null || card.currentHP < target.currentHP)
                target = card;
        }

        return target != null;
    }
}
