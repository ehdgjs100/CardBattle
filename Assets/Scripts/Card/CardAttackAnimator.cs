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

    public void PlaySpawnFromDeck(Vector3 originWorldPos, float duration = 0.35f)
    {
        _rect.DOKill();

        Vector3 targetPos = _rect.position;
        _rect.position = originWorldPos;
        _rect.localScale = _originalScale * 0.5f;

        _rect.DOMove(targetPos, duration).SetEase(Ease.OutBack);
        _rect.DOScale(_originalScale, duration).SetEase(Ease.OutBack);
    }
}
