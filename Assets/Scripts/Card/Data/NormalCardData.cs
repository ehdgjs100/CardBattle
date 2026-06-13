using UnityEngine;

[CreateAssetMenu(menuName = "Card/Normal")]
public class NormalCardData : CardDataBase
{
    public override CardEffect CreateEffect() => new NormalEffect();
}
