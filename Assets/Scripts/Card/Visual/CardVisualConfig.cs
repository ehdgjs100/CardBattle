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
    public GameObject receivedHitFXPrefab;
    public Vector2 receivedHitFXOffset;
    public Vector3 receivedHitFXRotation;
    public GameObject deathFXPrefab;
}
