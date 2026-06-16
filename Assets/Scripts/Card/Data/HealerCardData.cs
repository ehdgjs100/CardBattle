using UnityEngine;

[CreateAssetMenu(menuName = "Card/Healer")]
public class HealerCardData : CardDataBase
{
    public int healAmount = 1;

    public override CardType CardType => CardType.Healer;
    public override CardEffect CreateEffect() => new HealerEffect(healAmount);
}
