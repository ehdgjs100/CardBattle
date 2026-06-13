using UnityEngine;

[CreateAssetMenu(menuName = "Card/Ranged")]
public class RangedCardData : CardDataBase
{
    public override CardEffect CreateEffect() => new RangedEffect();
}
