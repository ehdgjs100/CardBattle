using DamageNumbersPro;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image typeIconImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private HPText hpText;
    [SerializeField] private GameObject frontRoot;
    [SerializeField] private GameObject cardBack;
    [SerializeField] private DamageNumber damageTextPrefab;

    public CardAttackAnimator AttackAnimator { get; private set; }

    private CardVisualConfig _visual;

    private void Awake()
    {
        AttackAnimator = GetComponent<CardAttackAnimator>();
    }

    public void Bind(CardInstance instance)
    {
        CardVisualConfig visual = instance.data.visual;
        _visual = visual;

        if (visual != null)
        {
            illustrationImage.sprite = visual.illustration;
            frameImage.color = visual.frameColor;
            typeIconImage.sprite = visual.typeIcon;
            typeIconImage.gameObject.SetActive(visual.typeIcon != null);
        }

        cardNameText.text = instance.data.cardName;
        hpText.Init(instance.data.maxHP, instance.currentHP);
        SetFaceDown(false);
    }

    public void SetFaceDown(bool faceDown)
    {
        frontRoot.SetActive(!faceDown);
        cardBack.SetActive(faceDown);
    }

    public void PlayAttackFX()
    {
        SpawnFX(_visual != null ? _visual.attackFXPrefab : null);
    }

    public void PlayHitFX()
    {
        SpawnFX(_visual != null ? _visual.hitFXPrefab : null);
    }

    public void PlayDamageText(int amount)
    {
        if (damageTextPrefab == null || amount <= 0)
            return;

        Vector3 position = transform.position + new Vector3(0.5f, 0.5f, FXDepthOffset);
        DamageNumber popup = damageTextPrefab.Spawn(position, amount);
        popup.UpdateText();

        Vector3 risePosition = position + Vector3.up * DamageRiseDistance;
        DOTween.To(() => popup.position, value => popup.position = value, risePosition, DamageRiseDuration)
            .SetEase(Ease.OutQuad);
    }

    private const float FXDepthOffset = -0.5f;
    private const float DamageRiseDistance = 1f;
    private const float DamageRiseDuration = 0.6f;

    private void SpawnFX(GameObject prefab)
    {
        if (prefab == null)
            return;

        Vector3 position = transform.position + new Vector3(0f, 0f, FXDepthOffset);
        Instantiate(prefab, position, Quaternion.identity);
    }
}
