using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TurnCoin : MonoBehaviour
{
    [SerializeField] private float playerY = -30f;
    [SerializeField] private float enemyY = 30f;
    [SerializeField] private float arcHeight = 280f;
    [SerializeField] private float duration = 0.65f;
    [SerializeField] private int spinCount = 3;

    private RectTransform _rect;

    private void Awake()
    {
        _rect = (RectTransform)transform;
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.PlayerSelectCard)
            FlipTo(playerY);
        else if (state == GameState.EnemyTurn)
            FlipTo(enemyY);
    }

    private void FlipTo(float targetY)
    {
        float peakY = Mathf.Max(_rect.anchoredPosition.y, targetY) + arcHeight;

        _rect.DOKill();

        Sequence arc = DOTween.Sequence();
        arc.Append(_rect.DOAnchorPosY(peakY, duration * 0.42f).SetEase(Ease.OutQuad));
        arc.Append(_rect.DOAnchorPosY(targetY, duration * 0.58f).SetEase(Ease.InQuad));

        _rect.DOLocalRotate(new Vector3(0f, 360f * spinCount, 0f), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutSine);
    }
}
