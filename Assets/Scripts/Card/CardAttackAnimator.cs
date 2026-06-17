using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CardAttackAnimator : MonoBehaviour
{
    [Header("Melee")]
    [SerializeField] private float lungeDuration = 0.15f;
    [SerializeField] private float lungeRatio = 0.6f;
    [SerializeField] private float attackPunchScale = 0.2f;

    [Header("Hit")]
    [SerializeField] private float hitPunchScale = 0.35f;
    [SerializeField] private float hitDuration = 0.3f;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakeStrength = 10f;
    [SerializeField] private int shakeVibrato = 15;

    [Header("Knockback")]
    [SerializeField] private float knockbackDist = 45f;
    [SerializeField] private float knockbackOutDuration = 0.08f;
    [SerializeField] private float knockbackBackDuration = 0.22f;

    [Header("Spawn")]
    [SerializeField] private float spawnDuration = 1f;
    [SerializeField] private float spawnOvershoot = 0.85f;

    [Header("Assassin")]
    [SerializeField] private float assassinDisappearDuration = 0.08f;
    [SerializeField] private float assassinAppearDuration = 0.1f;
    [SerializeField] private float assassinSlashDuration = 0.22f;
    [SerializeField] private float assassinSlashAngle = 40f;
    [SerializeField] private float assassinTargetRatio = 0.85f;

    public float SpawnDuration => spawnDuration;

    private RectTransform _rect;
    private Vector2 _originalAnchoredPos;
    private Vector3 _originalScale;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _originalAnchoredPos = _rect.anchoredPosition;
        _originalScale = _rect.localScale;
    }

    public void PlayMeleeAttack(Vector2 targetOffset, System.Action onImpact = null, System.Action onComplete = null)
    {
        _rect.DOKill();
        _rect.anchoredPosition = _originalAnchoredPos;
        _rect.localScale = _originalScale;

        Vector2 lungePos = _originalAnchoredPos + targetOffset * lungeRatio;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(_rect.DOAnchorPos(lungePos, lungeDuration).SetEase(Ease.OutQuad));
        sequence.AppendCallback(() =>
        {
            _rect.DOPunchScale(_originalScale * attackPunchScale, lungeDuration, 1, 0f);
            onImpact?.Invoke();
        });
        sequence.AppendInterval(lungeDuration);
        sequence.Append(_rect.DOAnchorPos(_originalAnchoredPos, lungeDuration).SetEase(Ease.InQuad));
        sequence.OnComplete(() => onComplete?.Invoke());
    }

    public void PlayAttackPulse()
    {
        _rect.DOKill();
        _rect.localScale = _originalScale;
        _rect.DOPunchScale(_originalScale * attackPunchScale, lungeDuration, 1, 0f);
    }

    public void PlayHitReaction(System.Action onComplete = null)
    {
        _rect.DOKill();
        _rect.localScale = _originalScale;
        _rect.DOPunchScale(_originalScale * hitPunchScale, hitDuration, 1, 0.6f)
            .OnComplete(() => onComplete?.Invoke());
    }

    public void PlayShake(System.Action onComplete = null)
    {
        _rect.DOKill();
        _rect.anchoredPosition = _originalAnchoredPos;
        _rect.DOShakeAnchorPos(shakeDuration, new Vector2(shakeStrength, 0f), shakeVibrato, 0f)
            .OnComplete(() =>
            {
                _rect.anchoredPosition = _originalAnchoredPos;
                onComplete?.Invoke();
            });
    }

    public void PlayAssassinAttack(Vector2 targetOffset, System.Action onImpact = null, System.Action onComplete = null)
    {
        _rect.DOKill();
        _rect.anchoredPosition = _originalAnchoredPos;
        _rect.localScale = _originalScale;
        _rect.localRotation = Quaternion.identity;

        Vector2 targetPos = _originalAnchoredPos + targetOffset * assassinTargetRatio;

        Sequence seq = DOTween.Sequence();
        seq.Append(_rect.DOScale(Vector3.zero, assassinDisappearDuration).SetEase(Ease.InQuad));
        seq.AppendCallback(() => _rect.anchoredPosition = targetPos);
        seq.Append(_rect.DOScale(_originalScale, assassinAppearDuration).SetEase(Ease.OutBack, 2f));
        seq.AppendCallback(() =>
        {
            _rect.DOPunchRotation(new Vector3(0f, 0f, -assassinSlashAngle), assassinSlashDuration, 1, 0.3f);
            onImpact?.Invoke();
        });
        seq.AppendInterval(assassinSlashDuration);
        seq.Append(_rect.DOScale(Vector3.zero, assassinDisappearDuration).SetEase(Ease.InQuad));
        seq.AppendCallback(() => _rect.anchoredPosition = _originalAnchoredPos);
        seq.Append(_rect.DOScale(_originalScale, assassinAppearDuration).SetEase(Ease.OutBack, 2f));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void PlayKnockback(float dirX)
    {
        Vector2 pushed = _originalAnchoredPos + new Vector2(dirX * knockbackDist, 0f);

        DOTween.Sequence()
            .SetTarget(_rect)
            .Append(_rect.DOAnchorPos(pushed, knockbackOutDuration).SetEase(Ease.OutQuad))
            .Append(_rect.DOAnchorPos(_originalAnchoredPos, knockbackBackDuration).SetEase(Ease.OutBack));
    }

    public void PlaySpawnFromDeck(Vector3 originWorldPos, System.Action onFlip = null, System.Action onComplete = null)
    {
        _rect.DOKill();

        Vector3 targetPos = _rect.position;
        _rect.position = originWorldPos;
        _rect.localScale = _originalScale * 0.5f;
        _rect.localRotation = Quaternion.identity;

        _rect.DOMove(targetPos, spawnDuration).SetEase(Ease.OutBack, spawnOvershoot);
        _rect.DOScale(_originalScale, spawnDuration).SetEase(Ease.OutBack, spawnOvershoot);

        float flipDuration = spawnDuration * 0.5f;
        Sequence flip = DOTween.Sequence();
        flip.Append(_rect.DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration).SetEase(Ease.InQuad));
        flip.AppendCallback(() => onFlip?.Invoke());
        flip.Append(_rect.DOLocalRotate(Vector3.zero, flipDuration).SetEase(Ease.OutQuad));
        flip.OnComplete(() => onComplete?.Invoke());
    }
}
