using UnityEngine;

[CreateAssetMenu(menuName = "Card/VisualConfig")]
public class CardVisualConfig : ScriptableObject
{
    public Sprite illustration;
    public Sprite typeIcon;
    public Color frameColor;
    public RuntimeAnimatorController animController;
    public GameObject attackFXPrefab;
    public GameObject hitFXPrefab;
    public GameObject deathFXPrefab;
}
