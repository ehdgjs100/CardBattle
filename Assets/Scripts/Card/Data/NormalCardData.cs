using UnityEngine;

[CreateAssetMenu(menuName = "Card/Normal")]
public class NormalCardData : CardDataBase
{
    public override CardType CardType => CardType.Normal;
    public override CardEffect CreateEffect() => new NormalEffect();
}
