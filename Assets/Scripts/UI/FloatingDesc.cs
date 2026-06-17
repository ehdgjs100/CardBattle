using TMPro;
using UnityEngine;

public class FloatingDesc : MonoBehaviour
{
    public static FloatingDesc Instance { get; private set; }

    [SerializeField] private TMP_Text featureText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Vector2 cursorOffset = new Vector2(20f, -20f);

    private RectTransform _rect;
    private Canvas _rootCanvas;

    private void Awake()
    {
        Instance = this;
        _rect = (RectTransform)transform;
        _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        Canvas selfCanvas = gameObject.GetComponent<Canvas>();
        if (selfCanvas == null) selfCanvas = gameObject.AddComponent<Canvas>();
        selfCanvas.overrideSorting = true;
        selfCanvas.sortingOrder = 999;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        Camera cam = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)_rootCanvas.transform, Input.mousePosition, cam, out Vector2 localPoint);
        _rect.anchoredPosition = localPoint + cursorOffset;
    }

    public void Show(string feature, string desc)
    {
        gameObject.SetActive(true);
        featureText?.SetText(feature);
        descText?.SetText(desc);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
