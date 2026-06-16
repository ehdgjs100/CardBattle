using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private BattleSlot[] playerSlots;
    [SerializeField] private BattleSlot[] enemySlots;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private WaitingCardCount playerWaitingCount;
    [SerializeField] private WaitingCardCount enemyWaitingCount;
    [SerializeField] private TurnBanner turnBanner;
    [SerializeField] private ResultPanel resultPanel;

    private bool _hasDealtInitialCards;

    private void Awake()
    {
        Instance = this;

        SpawnCardViews(playerSlots);
        SpawnCardViews(enemySlots);

        GameManager.Instance.OnStateChanged += HandleStateChanged;
        TurnManager.Instance.OnSelectionChanged += HandleSelectionChanged;
        BattleManager.Instance.OnAttackPerformed += HandleAttackPerformed;
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
        bool isFirstDeal = !_hasDealtInitialCards;

        RefreshField(playerSlots, GameManager.Instance.PlayerField);
        RefreshField(enemySlots, GameManager.Instance.EnemyField);
        _hasDealtInitialCards = true;

        if (isFirstDeal)
        {
            DOVirtual.DelayedCall(0f, () =>
            {
                DealCardsSequentially(playerSlots, playerWaitingCount, 0);
                DealCardsSequentially(enemySlots, enemyWaitingCount, 0);
            });
        }

        playerWaitingCount?.SetCount(GameManager.Instance.PlayerField.WaitingCount);
        enemyWaitingCount?.SetCount(GameManager.Instance.EnemyField.WaitingCount);

        switch (state)
        {
            case GameState.PlayerSelectCard:
                turnBanner?.Show("플레이어 턴");
                break;
            case GameState.EnemyTurn:
                turnBanner?.Show("적 턴");
                break;
            case GameState.Win:
            case GameState.Lose:
                resultPanel?.Show(state == GameState.Win ? GameResult.Win : GameResult.Lose);
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

            if (previous == next || next == null)
                continue;

            CardView cardView = slots[i].CardView;

            if (!_hasDealtInitialCards && previous == null)
            {
                cardView.gameObject.SetActive(false);
            }
            else if (previous != null && waitingCount != null)
            {
                cardView.SetFaceDown(true);
                cardView.AttackAnimator.PlaySpawnFromDeck(waitingCount.transform.position, () => cardView.SetFaceDown(false));
                TurnManager.Instance.NotifyCardSpawnAnimation(CardAttackAnimator.SpawnDuration);
            }
            else
            {
                cardView.SetFaceDown(false);
            }
        }
    }

    private void DealCardsSequentially(BattleSlot[] slots, WaitingCardCount deckSource, int index)
    {
        if (index >= slots.Length) return;

        BattleSlot slot = slots[index];
        if (slot.Card == null)
        {
            DealCardsSequentially(slots, deckSource, index + 1);
            return;
        }

        Vector3 deckPos = deckSource != null ? deckSource.transform.position : Vector3.zero;
        CardView cardView = slot.CardView;

        cardView.gameObject.SetActive(true);
        cardView.SetFaceDown(true);
        cardView.AttackAnimator.PlaySpawnFromDeck(deckPos, onFlip: () => cardView.SetFaceDown(false));

        DOVirtual.DelayedCall(TurnManager.Instance.EnemyTurnDelay,
            () => DealCardsSequentially(slots, deckSource, index + 1));
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

        attackerSlot.transform.SetAsLastSibling();
        attackerSlot.CardView.PlayAttackFX();

        if (result.Attacker.effect.IsMelee)
        {
            Vector2 offset = ((RectTransform)targetSlot.transform).anchoredPosition
                - ((RectTransform)attackerSlot.transform).anchoredPosition;

            attackerSlot.CardView.AttackAnimator.PlayMeleeAttack(
                offset,
                onImpact: () =>
                {
                    targetSlot.CardView.PlayHitFX();
                    targetSlot.CardView.AttackAnimator.PlayHitReaction();
                    targetSlot.CardView.PlayDamageText(result.DamageDealt);
                    attackerSlot.CardView.PlayDamageText(result.DamageReceived);

                    PlaySplashHits(result.SplashHits);
                },
                onComplete: onAnimationComplete);
        }
        else
        {
            attackerSlot.CardView.AttackAnimator.PlayAttackPulse();
            attackerSlot.CardView.PlayProjectile(
                targetSlot.transform.position,
                onArrive: () =>
                {
                    targetSlot.CardView.PlayHitFX();
                    targetSlot.CardView.AttackAnimator.PlayHitReaction(onAnimationComplete);
                    targetSlot.CardView.PlayDamageText(result.DamageDealt);
                    attackerSlot.CardView.PlayDamageText(result.DamageReceived);
                    PlaySplashHits(result.SplashHits);
                });
        }
    }

    private void PlaySplashHits(IReadOnlyList<SplashHit> splashHits)
    {
        for (int i = 0; i < splashHits.Count; i++)
        {
            BattleSlot slot = FindSlot(splashHits[i].Target);
            if (slot == null)
                continue;

            slot.CardView.PlayHitFX();
            slot.CardView.AttackAnimator.PlayHitReaction();
            slot.CardView.PlayDamageText(splashHits[i].Damage);
        }
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
