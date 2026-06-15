using System;
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
        RefreshField(playerSlots, GameManager.Instance.PlayerField);
        RefreshField(enemySlots, GameManager.Instance.EnemyField);
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
        for (int i = 0; i < slots.Length; i++)
            slots[i].Bind(field.Slots[i]);
    }

    private void HandleAttackPerformed(CardInstance attacker, CardInstance target, Action onAnimationComplete)
    {
        BattleSlot attackerSlot = FindSlot(attacker);
        BattleSlot targetSlot = FindSlot(target);

        if (attackerSlot == null || targetSlot == null)
        {
            onAnimationComplete?.Invoke();
            return;
        }

        attackerSlot.transform.SetAsLastSibling();

        if (attacker.effect.IsMelee)
        {
            Vector2 offset = ((RectTransform)targetSlot.transform).anchoredPosition
                - ((RectTransform)attackerSlot.transform).anchoredPosition;

            attackerSlot.CardView.AttackAnimator.PlayMeleeAttack(
                offset,
                onImpact: () => targetSlot.CardView.AttackAnimator.PlayHitReaction(),
                onComplete: onAnimationComplete);
        }
        else
        {
            attackerSlot.CardView.AttackAnimator.PlayAttackPulse();
            targetSlot.CardView.AttackAnimator.PlayHitReaction(onAnimationComplete);
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
