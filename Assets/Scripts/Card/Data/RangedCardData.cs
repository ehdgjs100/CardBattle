using UnityEngine;

[CreateAssetMenu(menuName = "Card/Ranged")]
public class RangedCardData : CardDataBase
{
    public override CardType CardType => CardType.Ranged;
    public override CardEffect CreateEffect() => new RangedEffect();
}
