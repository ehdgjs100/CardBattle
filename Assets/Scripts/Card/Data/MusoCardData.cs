using UnityEngine;

[CreateAssetMenu(menuName = "Card/Muso")]
public class MusoCardData : CardDataBase
{
    [Range(0f, 1f)] public float splashRatio = 0.5f;

    public override CardType CardType => CardType.Muso;
    public override CardEffect CreateEffect() => new MusoEffect(splashRatio);
}
