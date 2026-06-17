using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public struct CardTypeIconEntry
{
    public CardType type;
    public Sprite sprite;
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private BattleSlot[] playerSlots;
    [SerializeField] private BattleSlot[] enemySlots;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private WaitingCardCount playerWaitingCount;
    [SerializeField] private WaitingCardCount enemyWaitingCount;
    [SerializeField] private ResultPanel resultPanel;
    [SerializeField] private TurnCoin turnCoin;
    [SerializeField] private GameObject healFXPrefab;
    [SerializeField] private CardTypeIconEntry[] innerTypeIcons;
    [SerializeField] private TMP_Text turnCountText;
    [SerializeField] private RectTransform turnPanel;

    public bool IsInteractionLocked => _lockCount > 0;

    private bool _hasDealtInitialCards;
    private int _lockCount;
    private readonly HashSet<CardInstance> _healSubscribed = new HashSet<CardInstance>();
    private Vector3 _turnTextOrigLocalPos;

    private void Awake()
    {
        Instance = this;

        SpawnCardViews(playerSlots);
        SpawnCardViews(enemySlots);

        GameManager.Instance.OnStateChanged += HandleStateChanged;
        TurnManager.Instance.OnSelectionChanged += HandleSelectionChanged;
        BattleManager.Instance.OnAttackPerformed += HandleAttackPerformed;

        if (turnCountText != null)
            _turnTextOrigLocalPos = turnCountText.transform.localPosition;
    }

    private void Start()
    {
        PlayIntroAnimations();
    }

    private void PlayIntroAnimations()
    {
        if (turnPanel != null)
        {
            Vector2 orig = turnPanel.anchoredPosition;
            turnPanel.anchoredPosition = orig + Vector2.up * 600f;
            turnPanel.DOAnchorPos(orig, 1.0f).SetEase(Ease.OutCubic);
        }

        if (playerWaitingCount != null)
        {
            var rt = (RectTransform)playerWaitingCount.transform;
            Vector2 orig = rt.anchoredPosition;
            rt.anchoredPosition = orig + Vector2.down * 900f;
            rt.DOAnchorPos(orig, 0.4f).SetEase(Ease.OutBack, 1.4f).SetDelay(0.15f);
        }

        if (enemyWaitingCount != null)
        {
            var rt = (RectTransform)enemyWaitingCount.transform;
            Vector2 orig = rt.anchoredPosition;
            rt.anchoredPosition = orig + Vector2.up * 900f;
            rt.DOAnchorPos(orig, 0.4f).SetEase(Ease.OutBack, 1.4f).SetDelay(0.15f);
        }
    }

    private void SpawnCardViews(BattleSlot[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            CardView view = Instantiate(cardViewPrefab, slots[i].transform);
            view.transform.SetAsFirstSibling();
            slots[i].SetCardView(view);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;

        if (TurnManager.Instance != null)
            TurnManager.Instance.OnSelectionChanged -= HandleSelectionChanged;

        if (BattleManager.Instance != null)
            BattleManager.Instance.OnAttackPerformed -= HandleAttackPerformed;
    }

    private void HandleStateChanged(GameState state)
    {
        EventSystem.current?.SetSelectedGameObject(null);
        bool isFirstDeal = !_hasDealtInitialCards;

        RefreshField(playerSlots, GameManager.Instance.PlayerField);
        RefreshField(enemySlots, GameManager.Instance.EnemyField);
        _hasDealtInitialCards = true;

        if (isFirstDeal)
        {
            _lockCount += 2;
            StartCoroutine(DealCoroutine(playerSlots, playerWaitingCount));
            StartCoroutine(DealCoroutine(enemySlots, enemyWaitingCount));
        }

        playerWaitingCount?.SetCount(GameManager.Instance.PlayerField.WaitingCount);
        enemyWaitingCount?.SetCount(GameManager.Instance.EnemyField.WaitingCount);

        switch (state)
        {
            case GameState.PlayerSelectCard:
                PlayTurnTextChange(TurnManager.Instance.TurnNumber);
                break;
            case GameState.Win:
                resultPanel?.Show(GameResult.Win, BattleManager.Instance.TotalKills, TurnManager.Instance.TurnNumber);
                break;
            case GameState.Lose:
                resultPanel?.Show(GameResult.Lose, BattleManager.Instance.TotalKills, TurnManager.Instance.TurnNumber);
                break;
        }

        HandleSelectionChanged();
    }

    private void HandleSelectionChanged()
    {
        CardInstance selected = TurnManager.Instance.SelectedAttacker;

        for (int i = 0; i < playerSlots.Length; i++)
            playerSlots[i].SetHighlight(selected != null && playerSlots[i].Card == selected);
    }

    private void RefreshField(BattleSlot[] slots, CardField field)
    {
        WaitingCardCount waitingCount = field.Owner == Owner.Player ? playerWaitingCount : enemyWaitingCount;

        for (int i = 0; i < slots.Length; i++)
        {
            CardInstance previous = slots[i].Card;
            CardInstance next = field.Slots[i];

            slots[i].Bind(next);

            if (next != null && _healSubscribed.Add(next))
            {
                CardInstance c = next;
                next.OnHealQueued += amount => ShowHealFX(c, amount);
                next.OnHealCast += () => PlayHealCastAnimation(c);
            }

            if (previous == next || next == null)
                continue;

            CardView cardView = slots[i].CardView;

            if (!_hasDealtInitialCards && previous == null)
            {
                cardView.gameObject.SetActive(false);
            }
            else if (previous != null && waitingCount != null)
            {
                _lockCount++;
                cardView.SetFaceDown(true);
                cardView.AttackAnimator.PlaySpawnFromDeck(waitingCount.transform.position, () =>
                {
                    cardView.SetFaceDown(false);
                    _lockCount--;
                });
                TurnManager.Instance.NotifyCardSpawnAnimation(cardView.AttackAnimator.SpawnDuration);
            }
            else
            {
                cardView.SetFaceDown(false);
            }
        }
    }

    private IEnumerator DealCoroutine(BattleSlot[] slots, WaitingCardCount deckSource)
    {
        yield return null;

        float stagger = TurnManager.Instance.EnemyTurnDelay;
        Vector3 deckPos = deckSource != null ? deckSource.transform.position : Vector3.zero;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].Card == null) continue;

            CardView cardView = slots[i].CardView;
            cardView.gameObject.SetActive(true);
            cardView.SetFaceDown(true);
            cardView.AttackAnimator.PlaySpawnFromDeck(deckPos, onFlip: () => cardView.SetFaceDown(false));

            if (i < slots.Length - 1)
                yield return new WaitForSeconds(stagger);
        }

        _lockCount--;
    }

    private void HandleAttackPerformed(AttackResult result, Action onAnimationComplete)
    {
        BattleSlot attackerSlot = FindSlot(result.Attacker);
        BattleSlot targetSlot = FindSlot(result.Target);

        if (attackerSlot == null || targetSlot == null)
        {
            onAnimationComplete?.Invoke();
            return;
        }

        _lockCount++;
        attackerSlot.transform.SetAsLastSibling();
        attackerSlot.CardView.PlayAttackFX();

        Action onComplete = () =>
        {
            _lockCount--;
            EventSystem.current?.SetSelectedGameObject(null);
            turnCoin?.transform.SetAsLastSibling();
            onAnimationComplete?.Invoke();
        };

        bool targetDied = !result.Target.IsAlive;
        bool attackerDied = !result.Attacker.IsAlive;

        Action original = onComplete;
        Action playTargetDeath = null;
        Action playAttackerDeath = null;

        if (targetDied || attackerDied)
        {
            int remaining = (targetDied ? 1 : 0) + (attackerDied ? 1 : 0);
            Action onOne = () => { if (--remaining == 0) original?.Invoke(); };

            if (targetDied) playTargetDeath = () => targetSlot.CardView.PlayDeath(onOne);
            if (attackerDied) playAttackerDeath = () => attackerSlot.CardView.PlayDeath(onOne);
        }

        if (targetDied)
            onComplete = playTargetDeath;
        else if (attackerDied)
            onComplete = () => { };

        GameObject hitFX = attackerSlot.CardView.HitFXPrefab;

        if (result.Attacker.effect.IsAssassin)
        {
            Vector2 offset = ((RectTransform)targetSlot.transform).anchoredPosition
                - ((RectTransform)attackerSlot.transform).anchoredPosition;

            attackerSlot.CardView.AttackAnimator.PlayAssassinAttack(
                offset,
                onImpact: () =>
                {
                    targetSlot.CardView.PlayHitFX(hitFX);
                    targetSlot.CardView.PlayReceivedHitFX();
                    targetSlot.CardView.AttackAnimator.PlayHitReaction();
                    targetSlot.CardView.RefreshHP();
                    targetSlot.CardView.PlayDamageText(result.DamageDealt);
                    attackerSlot.CardView.RefreshHP();
                    attackerSlot.CardView.PlayDamageText(result.DamageReceived);
                    PlaySplashHits(result.SplashHits, targetSlot, hitFX);
                    playAttackerDeath?.Invoke();
                },
                onComplete: onComplete);
        }
        else if (result.Attacker.effect.IsMelee)
        {
            Vector2 offset = ((RectTransform)targetSlot.transform).anchoredPosition
                - ((RectTransform)attackerSlot.transform).anchoredPosition;

            attackerSlot.CardView.AttackAnimator.PlayMeleeAttack(
                offset,
                onImpact: () =>
                {
                    targetSlot.CardView.PlayHitFX(hitFX);
                    targetSlot.CardView.PlayReceivedHitFX();
                    targetSlot.CardView.AttackAnimator.PlayHitReaction();
                    targetSlot.CardView.RefreshHP();
                    targetSlot.CardView.PlayDamageText(result.DamageDealt);
                    attackerSlot.CardView.RefreshHP();
                    attackerSlot.CardView.PlayDamageText(result.DamageReceived);

                    PlaySplashHits(result.SplashHits, targetSlot, hitFX);
                    playAttackerDeath?.Invoke();
                },
                onComplete: onComplete);
        }
        else
        {
            attackerSlot.CardView.AttackAnimator.PlayAttackPulse();
            attackerSlot.CardView.PlayProjectile(
                targetSlot.transform.position,
                onArrive: () =>
                {
                    targetSlot.CardView.PlayHitFX(hitFX);
                    targetSlot.CardView.PlayReceivedHitFX();
                    targetSlot.CardView.AttackAnimator.PlayHitReaction(onComplete);
                    targetSlot.CardView.RefreshHP();
                    targetSlot.CardView.PlayDamageText(result.DamageDealt);
                    attackerSlot.CardView.RefreshHP();
                    attackerSlot.CardView.PlayDamageText(result.DamageReceived);
                    PlaySplashHits(result.SplashHits, targetSlot, hitFX);
                    playAttackerDeath?.Invoke();
                });
        }
    }

    private void PlaySplashHits(IReadOnlyList<SplashHit> splashHits, BattleSlot mainTargetSlot, GameObject hitFX)
    {
        float mainX = ((RectTransform)mainTargetSlot.transform).anchoredPosition.x;

        for (int i = 0; i < splashHits.Count; i++)
        {
            BattleSlot slot = FindSlot(splashHits[i].Target);
            if (slot == null)
                continue;

            float splashX = ((RectTransform)slot.transform).anchoredPosition.x;
            float dirX = Mathf.Sign(splashX - mainX);

            slot.CardView.PlayHitFX(hitFX);
            slot.CardView.PlayReceivedHitFX();
            slot.CardView.RefreshHP();
            slot.CardView.PlayDamageText(splashHits[i].Damage);

            if (!splashHits[i].Target.IsAlive)
                slot.CardView.PlayDeath(null);
            else
                slot.CardView.AttackAnimator.PlayKnockback(dirX);
        }
    }

    private void ShowHealFX(CardInstance card, int amount)
    {
        float delay = TurnManager.Instance.TurnStartVisualDelay;
        DOVirtual.DelayedCall(delay, () =>
        {
            card.ApplyHeal(amount);

            BattleSlot slot = FindSlot(card);
            if (slot == null) return;

            if (healFXPrefab != null)
                FXPool.Instance.Spawn(healFXPrefab, slot.transform.position + new Vector3(0f, 0f, -0.5f), Quaternion.identity);

            slot.CardView.RefreshHP();
            slot.CardView.PlayHealText(amount);
        });
    }

    private void PlayHealCastAnimation(CardInstance card)
    {
        float delay = TurnManager.Instance.TurnStartVisualDelay;
        DOVirtual.DelayedCall(delay, () =>
        {
            BattleSlot slot = FindSlot(card);
            slot?.CardView.AttackAnimator.PlayAttackPulse();
        });
    }

    private void PlayTurnTextChange(int turnNumber)
    {
        if (turnCountText == null) return;

        turnCountText.transform.DOKill();
        turnCountText.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            turnCountText.SetText("턴 " + turnNumber);
            turnCountText.transform.localScale = Vector3.one;
            turnCountText.transform.localPosition = _turnTextOrigLocalPos + new Vector3(0f, 80f, 0f);
            turnCountText.transform.DOLocalMoveY(_turnTextOrigLocalPos.y, 0.45f).SetEase(Ease.OutCubic);
        });
    }

    public void PlayTankerBlockReject()
    {
        for (int i = 0; i < enemySlots.Length; i++)
        {
            CardInstance card = enemySlots[i].Card;
            if (card != null && card.IsAlive && card.data.CardType == CardType.Tanker)
            {
                enemySlots[i].CardView.PlayReject();
                return;
            }
        }
    }

    public Sprite GetInnerTypeIcon(CardType type)
    {
        for (int i = 0; i < innerTypeIcons.Length; i++)
            if (innerTypeIcons[i].type == type)
                return innerTypeIcons[i].sprite;
        return null;
    }

    private BattleSlot FindSlot(CardInstance card)
    {
        for (int i = 0; i < playerSlots.Length; i++)
            if (playerSlots[i].Card == card)
                return playerSlots[i];

        for (int i = 0; i < enemySlots.Length; i++)
            if (enemySlots[i].Card == card)
                return enemySlots[i];

        return null;
    }
}
