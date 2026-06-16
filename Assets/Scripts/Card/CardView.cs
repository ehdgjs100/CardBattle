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
    [SerializeField] private Image innerTypeIconImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private HPText hpText;
    [SerializeField] private GameObject frontRoot;
    [SerializeField] private GameObject cardBack;
    [SerializeField] private DamageNumber damageTextPrefab;
    [SerializeField] private DamageNumber healTextPrefab;
    [SerializeField] private Projectile projectilePrefab;

    public CardAttackAnimator AttackAnimator { get; private set; }
    public CardDeathAnimator DeathAnimator { get; private set; }
    public GameObject HitFXPrefab => _visual != null ? _visual.hitFXPrefab : null;

    private CardVisualConfig _visual;
    private CardInstance _boundInstance;

    private void Awake()
    {
        AttackAnimator = GetComponent<CardAttackAnimator>();
        DeathAnimator = GetComponent<CardDeathAnimator>();
    }

    public void PlayDeath(System.Action onComplete)
    {
        if (DeathAnimator != null)
            DeathAnimator.Play(onComplete);
        else
            onComplete?.Invoke();
    }

    public void Bind(CardInstance instance)
    {
        _boundInstance = instance;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null && cg) cg.alpha = 1f;

        CardVisualConfig visual = instance.data.visual;
        _visual = visual;

        if (visual != null)
        {
            illustrationImage.sprite = visual.illustration;
            frameImage.color = visual.frameColor;
            typeIconImage.sprite = visual.typeIcon;
            typeIconImage.gameObject.SetActive(visual.typeIcon != null);
        }

        if (innerTypeIconImage != null)
        {
            Sprite inner = UIManager.Instance.GetInnerTypeIcon(instance.data.CardType);
            innerTypeIconImage.sprite = inner;
            innerTypeIconImage.gameObject.SetActive(inner != null);
        }

        cardNameText.text = instance.data.cardName;
        hpText.Init(instance.data.maxHP, instance.currentHP);
    }

    public void RefreshHP()
    {
        if (_boundInstance != null)
            hpText.SetHP(_boundInstance.currentHP);
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

    public void PlayHitFX(GameObject prefab)
    {
        SpawnFX(prefab);
    }

    public void PlayDamageText(int amount)
    {
        if (damageTextPrefab == null || amount <= 0)
            return;

        Vector3 position = transform.position + new Vector3(0.5f, 0.5f, FXDepthOffset);
        DamageNumber popup = damageTextPrefab.Spawn(position, amount);
        popup.UpdateText();

        DOTween.To(() => popup.position, value => popup.position = value,
            position + Vector3.up * DamageRiseDistance, DamageRiseDuration).SetEase(Ease.OutQuad);
    }

    public void PlayHealText(int amount)
    {
        if (healTextPrefab == null || amount <= 0)
            return;

        Vector3 position = transform.position + new Vector3(0.5f, -0.5f, FXDepthOffset);
        DamageNumber popup = healTextPrefab.Spawn(position, amount);
        popup.enableLeftText = true;
        popup.leftText = "+";
        popup.UpdateText();

        DOTween.To(() => popup.position, value => popup.position = value,
            position + Vector3.up * DamageRiseDistance, DamageRiseDuration).SetEase(Ease.OutQuad);
    }

    public void PlayProjectile(Vector3 targetWorldPos, System.Action onArrive)
    {
        if (projectilePrefab == null)
        {
            onArrive?.Invoke();
            return;
        }

        Vector3 from = transform.position + new Vector3(0f, 0f, FXDepthOffset);
        Vector3 to = targetWorldPos + new Vector3(0f, 0f, FXDepthOffset);
        Instantiate(projectilePrefab, from, Quaternion.identity).Launch(from, to, onArrive);
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
