using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private Button retryButton;

    private bool _hasDealtInitialCards;
    private readonly HashSet<CardInstance> _healSubscribed = new HashSet<CardInstance>();

    private void Awake()
    {
        Instance = this;

        SpawnCardViews(playerSlots);
        SpawnCardViews(enemySlots);

        GameManager.Instance.OnStateChanged += HandleStateChanged;
        TurnManager.Instance.OnSelectionChanged += HandleSelectionChanged;
        BattleManager.Instance.OnAttackPerformed += HandleAttackPerformed;

        retryButton?.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
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
            StartCoroutine(DealCoroutine(playerSlots, playerWaitingCount));
            StartCoroutine(DealCoroutine(enemySlots, enemyWaitingCount));
        }

        playerWaitingCount?.SetCount(GameManager.Instance.PlayerField.WaitingCount);
        enemyWaitingCount?.SetCount(GameManager.Instance.EnemyField.WaitingCount);

        switch (state)
        {
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

        Action onComplete = () =>
        {
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

        // onComplete fires when attack animation ends:
        // - target death starts here (after attacker returns)
        // - attacker death starts at onImpact/onArrive so don't double-fire original
        if (targetDied)
            onComplete = playTargetDeath;
        else if (attackerDied)
            onComplete = () => { }; // attacker death handles original via onOne

        GameObject hitFX = attackerSlot.CardView.HitFXPrefab;

        if (result.Attacker.effect.IsMelee)
        {
            Vector2 offset = ((RectTransform)targetSlot.transform).anchoredPosition
                - ((RectTransform)attackerSlot.transform).anchoredPosition;

            attackerSlot.CardView.AttackAnimator.PlayMeleeAttack(
                offset,
                onImpact: () =>
                {
                    targetSlot.CardView.PlayHitFX(hitFX);
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
                Instantiate(healFXPrefab, slot.transform.position + new Vector3(0f, 0f, -0.5f), Quaternion.identity);

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
