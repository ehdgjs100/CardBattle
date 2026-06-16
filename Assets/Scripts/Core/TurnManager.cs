using System;
using DG.Tweening;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private float enemyTurnDelay = 0.9f;
    [SerializeField] private float afterCoinDelay = 0.3f;
    [SerializeField] private TurnCoin turnCoin;

    private CardField _playerField;
    private CardField _enemyField;
    private CardInstance _selectedAttacker;
    private float _pendingSpawnDelay;

    public float EnemyTurnDelay => enemyTurnDelay;
    public CardInstance SelectedAttacker => _selectedAttacker;
    public event Action OnSelectionChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void StartBattle(CardField playerField, CardField enemyField)
    {
        _playerField = playerField;
        _enemyField = enemyField;
        StartTurn(Owner.Player);
    }

    public void StartTurn(Owner turnOwner)
    {
        _selectedAttacker = null;
        OnSelectionChanged?.Invoke();

        CardField allyField = turnOwner == Owner.Player ? _playerField : _enemyField;
        TriggerTurnStartEffects(allyField);

        if (turnOwner == Owner.Player)
        {
            _pendingSpawnDelay = 0f;
            GameManager.Instance.SetState(GameState.PlayerSelectCard);
        }
        else
        {
            GameManager.Instance.SetState(GameState.EnemyTurn);
            float coinDelay = turnCoin != null ? turnCoin.Duration + afterCoinDelay : 0f;
            float delay = Mathf.Max(enemyTurnDelay, _pendingSpawnDelay, coinDelay);
            _pendingSpawnDelay = 0f;
            DOVirtual.DelayedCall(delay, RunEnemyTurn);
        }
    }

    public void NotifyCardSpawnAnimation(float duration)
    {
        _pendingSpawnDelay = Mathf.Max(_pendingSpawnDelay, duration);
    }

    public bool SelectAttacker(CardInstance card)
    {
        if (GameManager.Instance.CurrentState != GameState.PlayerSelectCard)
            return false;

        if (card == null || card.owner != Owner.Player || !card.IsAlive)
            return false;

        _selectedAttacker = card;
        OnSelectionChanged?.Invoke();
        return true;
    }

    public bool SelectTarget(CardInstance target)
    {
        if (GameManager.Instance.CurrentState != GameState.PlayerSelectCard)
            return false;

        if (_selectedAttacker == null)
            return false;

        if (target == null || target.owner != Owner.Enemy || !target.IsAlive)
            return false;

        GameManager.Instance.SetState(GameState.ApplyEffect);
        CardInstance attacker = _selectedAttacker;
        _selectedAttacker = null;
        OnSelectionChanged?.Invoke();

        BattleManager.Instance.ApplyAttack(attacker, target, _playerField, _enemyField, () => EndTurn(Owner.Player));
        return true;
    }

    private void RunEnemyTurn()
    {
        if (EnemyAI.TryDecideAction(_enemyField, _playerField, out CardInstance attacker, out CardInstance target))
            BattleManager.Instance.ApplyAttack(attacker, target, _enemyField, _playerField, () => EndTurn(Owner.Enemy));
        else
            EndTurn(Owner.Enemy);
    }

    private void EndTurn(Owner turnOwner)
    {
        GameManager.Instance.SetState(GameState.CheckResult);

        GameResult result = BattleManager.Instance.CheckResult(_playerField, _enemyField);
        switch (result)
        {
            case GameResult.Win:
                GameManager.Instance.SetState(GameState.Win);
                break;
            case GameResult.Lose:
                GameManager.Instance.SetState(GameState.Lose);
                break;
            default:
                StartTurn(turnOwner == Owner.Player ? Owner.Enemy : Owner.Player);
                break;
        }
    }

    private void TriggerTurnStartEffects(CardField field)
    {
        for (int i = 0; i < CardField.SlotCount; i++)
        {
            CardInstance card = field.Slots[i];
            if (card != null && card.IsAlive)
                card.effect.OnTurnStart(card, field.Slots);
        }
    }
}
