using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CardAttackAnimator : MonoBehaviour
{
    [SerializeField] private float lungeDuration = 0.15f;
    [SerializeField] private float lungeRatio = 0.6f;
    [SerializeField] private float attackPunchScale = 0.2f;
    [SerializeField] private float hitPunchScale = 0.35f;
    [SerializeField] private float hitDuration = 0.3f;

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

    public void PlayKnockback(float dirX)
    {
        const float dist = 45f;
        const float outDur = 0.08f;
        const float backDur = 0.22f;

        Vector2 pushed = _originalAnchoredPos + new Vector2(dirX * dist, 0f);

        DOTween.Sequence()
            .SetTarget(_rect)
            .Append(_rect.DOAnchorPos(pushed, outDur).SetEase(Ease.OutQuad))
            .Append(_rect.DOAnchorPos(_originalAnchoredPos, backDur).SetEase(Ease.OutBack));
    }

    public const float SpawnDuration = 1f;
    private const float DefaultBackOvershoot = 1.70158f;
    private const float SpawnOvershoot = DefaultBackOvershoot * 0.5f;

    public void PlaySpawnFromDeck(Vector3 originWorldPos, System.Action onFlip = null, System.Action onComplete = null, float duration = SpawnDuration)
    {
        _rect.DOKill();

        Vector3 targetPos = _rect.position;
        _rect.position = originWorldPos;
        _rect.localScale = _originalScale * 0.5f;
        _rect.localRotation = Quaternion.identity;

        _rect.DOMove(targetPos, duration).SetEase(Ease.OutBack, SpawnOvershoot);
        _rect.DOScale(_originalScale, duration).SetEase(Ease.OutBack, SpawnOvershoot);

        float flipDuration = duration * 0.5f;
        Sequence flip = DOTween.Sequence();
        flip.Append(_rect.DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration).SetEase(Ease.InQuad));
        flip.AppendCallback(() => onFlip?.Invoke());
        flip.Append(_rect.DOLocalRotate(Vector3.zero, flipDuration).SetEase(Ease.OutQuad));
        flip.OnComplete(() => onComplete?.Invoke());
    }
}
