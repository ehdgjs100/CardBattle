using UnityEngine;

[CreateAssetMenu(menuName = "Card/Assassin")]
public class AssassinCardData : CardDataBase
{
    public override CardType CardType => CardType.Assassin;
    public override CardEffect CreateEffect() => new AssassinEffect();
}
