using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private float animDuration = 0.3f;

    private CanvasGroup _canvasGroup;
    private RectTransform _rect;
    private Vector2 _shownPos;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _shownPos = _rect.anchoredPosition;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        closeButton?.onClick.AddListener(Hide);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _rect.anchoredPosition = _shownPos + Vector2.down * 1400f;
        _canvasGroup.alpha = 0f;

        _rect.DOAnchorPos(_shownPos, animDuration).SetEase(Ease.OutCubic);
        _canvasGroup.DOFade(1f, animDuration * 0.6f);
    }

    public void Hide()
    {
        _rect.DOAnchorPos(_shownPos + Vector2.down * 1400f, animDuration).SetEase(Ease.InCubic);
        _canvasGroup.DOFade(0f, animDuration * 0.5f)
            .OnComplete(() => gameObject.SetActive(false));
    }
}
