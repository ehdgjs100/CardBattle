using DamageNumbersPro;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image frameOutline;
    [SerializeField] private Image typeIconImage;
    [SerializeField] private Image innerTypeIconImage;
    [SerializeField] private Image rejectBorderImage;
    [SerializeField] private Image rarityBorderImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescText;
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

        if (frameOutline != null)
            frameOutline.transform.DOLocalRotate(new Vector3(0f, 0f, -360f), 10f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
    }

    public void PlayDeath(System.Action onComplete)
    {
        SpawnDeathFX();
        if (DeathAnimator != null)
            DeathAnimator.Play(onComplete);
        else
            onComplete?.Invoke();
    }

    private void SpawnDeathFX()
    {
        if (_visual == null || _visual.deathFXPrefab == null) return;

        Vector3 position = transform.position + new Vector3(0f, 0f, FXDepthOffset);
        GameObject fx = FXPool.Instance.Spawn(_visual.deathFXPrefab, position, Quaternion.identity);

        if (fx != null)
            foreach (Renderer r in fx.GetComponentsInChildren<Renderer>(true))
                r.sortingOrder = 100;
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

        cardNameText.text = instance.data.UpgradeLevel > 0
            ? instance.data.cardName + "+"
            : instance.data.cardName;
        cardNameText.color = instance.data.GetRarityColor();
        if (cardDescText != null)
            cardDescText.text = instance.data.feature;
        hpText.Init(instance.data.maxHP, instance.currentHP);

        if (rarityBorderImage != null)
            rarityBorderImage.color = instance.data.GetRarityColor();
    }

    public void PlayReject()
    {
        if (rejectBorderImage != null)
            rejectBorderImage.gameObject.SetActive(true);

        AttackAnimator.PlayShake(() =>
        {
            if (rejectBorderImage != null)
                rejectBorderImage.gameObject.SetActive(false);
        });
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

    public void PlayReceivedHitFX()
    {
        if (_visual == null || _visual.receivedHitFXPrefab == null)
            return;

        Vector2 offset = _visual.receivedHitFXOffset;
        Vector3 position = transform.position + new Vector3(offset.x, offset.y, FXDepthOffset);
        Quaternion rotation = Quaternion.Euler(_visual.receivedHitFXRotation);
        FXPool.Instance.Spawn(_visual.receivedHitFXPrefab, position, rotation);
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

    private const float LongPressDuration = 0.4f;
    private Coroutine _longPressCoroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_boundInstance == null) return;
        _longPressCoroutine = StartCoroutine(LongPressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_longPressCoroutine != null)
        {
            StopCoroutine(_longPressCoroutine);
            _longPressCoroutine = null;
        }
        FloatingDesc.Instance?.Hide();
    }

    private System.Collections.IEnumerator LongPressRoutine()
    {
        yield return new WaitForSeconds(LongPressDuration);
        FloatingDesc.Instance?.Show(_boundInstance.data.feature, _boundInstance.data.description);
        _longPressCoroutine = null;
    }

    private const float FXDepthOffset = -0.5f;
    private const float DamageRiseDistance = 1f;
    private const float DamageRiseDuration = 0.6f;

    private void SpawnFX(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 position = transform.position + new Vector3(0f, 0f, FXDepthOffset);
        FXPool.Instance.Spawn(prefab, position, Quaternion.identity);
    }
}
