using UnityEngine;

[CreateAssetMenu(menuName = "Card/Tanker")]
public class TankerCardData : CardDataBase
{
    public override CardType CardType => CardType.Tanker;
    public override CardEffect CreateEffect() => new TankerEffect();
}
