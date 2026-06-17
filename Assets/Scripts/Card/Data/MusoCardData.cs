using UnityEngine;

[CreateAssetMenu(menuName = "Card/Muso")]
public class MusoCardData : CardDataBase
{
    [Range(0f, 1f)] public float splashRatio = 0.5f;
    [Range(0f, 0.5f)] public float splashRatioPerUpgrade = 0.05f;

    public override CardType CardType => CardType.Muso;
    public override CardEffect CreateEffect() => new MusoEffect(splashRatio);

    public override void ApplyUpgrade(int level)
    {
        base.ApplyUpgrade(level);
        splashRatio = Mathf.Clamp01(splashRatio + splashRatioPerUpgrade * level);
    }
}
