using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private CanvasGroup dimOverlay;
    [SerializeField] private BattleSlot[] playerSlots;
    [SerializeField] private BattleSlot[] enemySlots;

    [Header("Decks")]
    [SerializeField] private List<CardDataBase> playerDeck;
    [SerializeField] private List<CardDataBase> enemyDeck;

    [Header("Step Config")]
    [SerializeField] private int tutorialPlayerSlotIndex = 0;
    [SerializeField] private int tutorialEnemySlotIndex = 0;

    [Header("Tutorial Text")]
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private string playerSelectMessage = "공격할 카드를 선택하세요";
    [SerializeField] private string enemySelectMessage = "공격할 적 카드를 선택하세요";
    private const string SkipMessage = "Skip";

    [Header("Info Sets")]
    [SerializeField] private GameObject attributeTutoSet;
    [SerializeField] private GameObject hpTutoSet;
    [SerializeField] private float clickCooldown = 1f;

    [Header("Settings")]
    [SerializeField] private float activationDelay = 1.5f;
    [SerializeField] private float dimAlpha = 0.7f;
    [SerializeField] private float cardRiseAmount = 100f;

    public List<CardDataBase> PlayerDeck => playerDeck;
    public List<CardDataBase> EnemyDeck => enemyDeck;
    public bool IsActive { get; private set; }

    private BattleSlot _allowedSlot;
    private BattleSlot _raisedSlot;
    private Transform _raisedOriginalParent;
    private int _raisedOriginalSiblingIndex;
    private Vector2 _raisedOriginalAnchoredPos;
    private bool _started;
    private int _postTutorialCheckResultCount;

    private bool _waitingForAttributeClick;
    private bool _waitingForHpClick;
    private bool _canClick;

    private void Awake()
    {
        Instance = this;
        if (!IsTutorialMode()) { gameObject.SetActive(false); return; }
        dimOverlay.gameObject.SetActive(false);
        if (attributeTutoSet != null) attributeTutoSet.SetActive(false);
        if (hpTutoSet != null) hpTutoSet.SetActive(false);
    }

    private void Start()
    {
        if (!IsTutorialMode()) return;
        GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || !_canClick) return;

        if (_waitingForAttributeClick)
        {
            _canClick = false;
            _waitingForAttributeClick = false;

            attributeTutoSet?.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (attributeTutoSet != null) attributeTutoSet.SetActive(false);
                    ShowHpTuto();
                });
        }
        else if (_waitingForHpClick)
        {
            _canClick = false;
            _waitingForHpClick = false;

            hpTutoSet?.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (hpTutoSet != null) hpTutoSet.SetActive(false);
                    HideText();
                    dimOverlay.DOFade(0f, 0.3f).OnComplete(() =>
                    {
                        dimOverlay.gameObject.SetActive(false);
                        tutorialCanvas.gameObject.SetActive(false);
                        PlayerPrefs.SetInt("TutorialDone", 1);
                        PlayerPrefs.Save();
                    });
                });
        }
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.PlayerSelectCard && !_started)
        {
            _started = true;
            StartCoroutine(ActivateRoutine());
        }
    }

    private IEnumerator ActivateRoutine()
    {
        yield return new WaitForSeconds(activationDelay);
        IsActive = true;

        dimOverlay.gameObject.SetActive(true);
        dimOverlay.alpha = 0f;
        dimOverlay.DOFade(dimAlpha, 0.4f)
            .OnComplete(() => RaiseSlot(playerSlots[tutorialPlayerSlotIndex], isPlayer: true));
    }

    private void RaiseSlot(BattleSlot slot, bool isPlayer)
    {
        _raisedSlot = slot;
        _allowedSlot = slot;

        RectTransform rt = (RectTransform)slot.transform;
        _raisedOriginalParent = slot.transform.parent;
        _raisedOriginalSiblingIndex = slot.transform.GetSiblingIndex();
        _raisedOriginalAnchoredPos = rt.anchoredPosition;

        slot.transform.SetParent(tutorialCanvas.transform, worldPositionStays: true);

        Vector2 direction = isPlayer ? Vector2.up : Vector2.down;
        rt.DOAnchorPos(rt.anchoredPosition + direction * cardRiseAmount, 0.3f)
            .SetEase(Ease.OutCubic);

        ShowText(isPlayer ? playerSelectMessage : enemySelectMessage);
    }

    private void LowerAndRestore(System.Action onComplete = null)
    {
        if (_raisedSlot == null) { onComplete?.Invoke(); return; }

        BattleSlot slot = _raisedSlot;
        Transform originalParent = _raisedOriginalParent;
        int originalSiblingIndex = _raisedOriginalSiblingIndex;
        Vector2 originalAnchoredPos = _raisedOriginalAnchoredPos;
        _raisedSlot = null;

        RectTransform rt = (RectTransform)slot.transform;

        // 즉시 원래 Canvas로 복원 → 이후 카드 생성이 메인 Canvas에서 일어남
        slot.transform.SetParent(originalParent, worldPositionStays: true);
        slot.transform.SetSiblingIndex(originalSiblingIndex);

        // 메인 Canvas 안에서 원위치로 애니메이션
        rt.DOAnchorPos(originalAnchoredPos, 0.2f)
            .SetEase(Ease.InCubic)
            .OnComplete(() => onComplete?.Invoke());
    }

    public void OnAttackerSelected()
    {
        HideText();
        LowerAndRestore(() =>
            DOVirtual.DelayedCall(0.1f, () =>
                RaiseSlot(enemySlots[tutorialEnemySlotIndex], isPlayer: false)));
    }

    public void OnTargetSelected()
    {
        HideText();
        _allowedSlot = null;
        IsActive = false;

        dimOverlay.DOFade(0f, 0.3f).OnComplete(() => dimOverlay.gameObject.SetActive(false));

        // 즉시 슬롯 복원 → 새 카드가 메인 Canvas에 생성됨
        LowerAndRestore(() => RestoreSlotOrder());

        // PlayerPrefs.SetInt("TutorialDone", 1); // 테스트 중 비활성화
        // PlayerPrefs.Save();

        _postTutorialCheckResultCount = 0;
        GameManager.Instance.OnStateChanged += WaitForPostTutorialEnd;
    }

    private void WaitForPostTutorialEnd(GameState state)
    {
        if (state != GameState.CheckResult) return;
        _postTutorialCheckResultCount++;
        if (_postTutorialCheckResultCount >= 2)
        {
            GameManager.Instance.OnStateChanged -= WaitForPostTutorialEnd;
            ShowAttributeTuto();
        }
    }

    private void ShowAttributeTuto()
    {
        dimOverlay.gameObject.SetActive(true);
        dimOverlay.alpha = 0f;
        dimOverlay.DOFade(dimAlpha, 0.3f).OnComplete(() =>
        {
            if (attributeTutoSet != null)
            {
                attributeTutoSet.SetActive(true);
                attributeTutoSet.transform.localScale = Vector3.zero;
                attributeTutoSet.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
            ShowText(SkipMessage);
            _canClick = false;
            DOVirtual.DelayedCall(clickCooldown, () =>
            {
                _waitingForAttributeClick = true;
                _canClick = true;
            });
        });
    }

    private void ShowHpTuto()
    {
        if (hpTutoSet == null) return;
        hpTutoSet.SetActive(true);
        hpTutoSet.transform.localScale = Vector3.zero;
        hpTutoSet.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)
            .OnComplete(() => DOVirtual.DelayedCall(clickCooldown, () =>
            {
                _waitingForHpClick = true;
                _canClick = true;
            }));
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

    private void RestoreSlotOrder()
    {
        if (enemySlots == null || playerSlots == null) return;
        Transform parent = enemySlots[0].transform.parent;

        for (int i = 0; i < enemySlots.Length; i++)
            if (enemySlots[i].transform.parent == parent)
                enemySlots[i].transform.SetSiblingIndex(i);

        for (int i = 0; i < playerSlots.Length; i++)
            if (playerSlots[i].transform.parent == parent)
                playerSlots[i].transform.SetSiblingIndex(enemySlots.Length + i);
    }

    public bool IsSlotAllowed(BattleSlot slot) => slot == _allowedSlot;

    public static bool IsTutorialMode() => !PlayerPrefs.HasKey("TutorialDone");
}
