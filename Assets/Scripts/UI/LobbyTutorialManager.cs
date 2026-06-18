using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTutorialManager : MonoBehaviour
{
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private CanvasGroup dimOverlay;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private float dimAlpha = 0.7f;
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private float dimFadeInDuration = 0.4f;
    [SerializeField] private float dimFadeOutDuration = 0.3f;

    [Header("Step 1 - Shop Button")]
    [SerializeField] private Button shopButton;
    [SerializeField] private float shopPanelOpenDelay = 0.4f;

    [Header("Step 2 - Chest")]
    [SerializeField] private RectTransform chest1Image;
    [SerializeField] private Button draw1Button;

    [Header("Step 3 - Result")]
    [SerializeField] private RectTransform resultPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button shopCloseButton;
    [SerializeField] private float shopPanelCloseDelay = 0.4f;

    [Header("Step 4 - Card Edit Button")]
    [SerializeField] private Button cardEditButton;

    private RaiseData _shopButtonData;
    private RaiseData _chest1ImageData;
    private RaiseData _resultPanelData;
    private RaiseData _shopCloseButtonData;
    private RaiseData _cardEditButtonData;

    private bool _step2Active;
    private bool _step3Active;
    private bool _waitingForShopClose;

    private struct RaiseData
    {
        public Transform OrigParent;
        public int OrigSiblingIndex;
        public Vector2 OrigAnchoredPos;
    }

    private void Awake()
    {
        if (!IsLobbyTutorialPending()) { gameObject.SetActive(false); return; }
        dimOverlay.gameObject.SetActive(false);
        if (tutorialText != null) tutorialText.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (!IsLobbyTutorialPending()) return;
        shopButton.onClick.AddListener(OnShopButtonClicked);
        draw1Button.onClick.AddListener(OnDraw1ButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        shopCloseButton.onClick.AddListener(OnShopCloseButtonClicked);
        cardEditButton.onClick.AddListener(OnCardEditButtonClicked);
        DOVirtual.DelayedCall(startDelay, StartStep1);
    }

    private void OnDisable()
    {
        shopButton?.onClick.RemoveListener(OnShopButtonClicked);
        draw1Button?.onClick.RemoveListener(OnDraw1ButtonClicked);
        confirmButton?.onClick.RemoveListener(OnConfirmButtonClicked);
        shopCloseButton?.onClick.RemoveListener(OnShopCloseButtonClicked);
        cardEditButton?.onClick.RemoveListener(OnCardEditButtonClicked);
    }

    // Step 1 — dim + 상점 버튼
    private void StartStep1()
    {
        ShowDim(() =>
        {
            _shopButtonData = Raise((RectTransform)shopButton.transform);
            ShowText("상점으로 이동합니다");
        });
    }

    private void OnShopButtonClicked()
    {
        HideText();
        Restore((RectTransform)shopButton.transform, _shopButtonData);
        DOVirtual.DelayedCall(shopPanelOpenDelay, StartStep2);
    }

    // Step 2 — chest1 이미지 + draw1 버튼
    private void StartStep2()
    {
        _chest1ImageData = Raise(chest1Image);
        _step2Active = true;
        ShowText("상자를 오픈합니다");
    }

    private void OnDraw1ButtonClicked()
    {
        if (!_step2Active) return;
        _step2Active = false;
        HideText();
        Restore(chest1Image, _chest1ImageData);
        DOVirtual.DelayedCall(0.05f, StartStep3);
    }

    // Step 3 — 결과 패널
    private void StartStep3()
    {
        _resultPanelData = Raise(resultPanel);
        _step3Active = true;
        ShowText("로비로 나갑니다");
    }

    private void OnConfirmButtonClicked()
    {
        if (!_step3Active) return;
        _step3Active = false;
        HideText();
        Restore(resultPanel, _resultPanelData);
        _shopCloseButtonData = Raise((RectTransform)shopCloseButton.transform);
        _waitingForShopClose = true;
    }

    private void OnShopCloseButtonClicked()
    {
        if (!_waitingForShopClose) return;
        _waitingForShopClose = false;
        Restore((RectTransform)shopCloseButton.transform, _shopCloseButtonData);
        DOVirtual.DelayedCall(shopPanelCloseDelay, StartStep4);
    }

    // Step 4 — 카드 편집 버튼
    private void StartStep4()
    {
        _cardEditButtonData = Raise((RectTransform)cardEditButton.transform);
    }

    private void OnCardEditButtonClicked()
    {
        Restore((RectTransform)cardEditButton.transform, _cardEditButtonData);
        HideDim(() =>
        {
            PlayerPrefs.SetInt("LobbyTutorialDone", 1);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        });
    }

    // 공통 유틸
    private void ShowDim(System.Action onComplete = null)
    {
        dimOverlay.gameObject.SetActive(true);
        dimOverlay.alpha = 0f;
        dimOverlay.DOFade(dimAlpha, dimFadeInDuration).OnComplete(() => onComplete?.Invoke());
    }

    private void HideDim(System.Action onComplete = null)
    {
        dimOverlay.DOFade(0f, dimFadeOutDuration).OnComplete(() =>
        {
            dimOverlay.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void ShowText(string message)
    {
        if (tutorialText == null) return;
        tutorialText.text = message;
        tutorialText.gameObject.SetActive(true);
        tutorialText.DOFade(1f, 0.3f).From(0f);
    }

    private void HideText()
    {
        if (tutorialText == null) return;
        tutorialText.DOFade(0f, 0.2f)
            .OnComplete(() => tutorialText.gameObject.SetActive(false));
    }

    private RaiseData Raise(RectTransform rt)
    {
        RaiseData data = new RaiseData
        {
            OrigParent = rt.parent,
            OrigSiblingIndex = rt.GetSiblingIndex(),
            OrigAnchoredPos = rt.anchoredPosition
        };
        rt.SetParent(tutorialCanvas.transform, worldPositionStays: true);
        return data;
    }

    private static void Restore(RectTransform rt, RaiseData data)
    {
        rt.SetParent(data.OrigParent, worldPositionStays: true);
        rt.SetSiblingIndex(data.OrigSiblingIndex);
        rt.anchoredPosition = data.OrigAnchoredPos;
    }

    public static bool IsLobbyTutorialPending() =>
        PlayerPrefs.HasKey("TutorialDone") && !PlayerPrefs.HasKey("LobbyTutorialDone");
}
