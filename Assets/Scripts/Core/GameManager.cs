using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<CardDataBase> playerDeck;
    [SerializeField] private List<CardDataBase> enemyDeck;

    public CardField PlayerField { get; private set; }
    public CardField EnemyField { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Init;

    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PlayerField = new CardField(playerDeck, Owner.Player);
        EnemyField = new CardField(enemyDeck, Owner.Enemy);

        TurnManager.Instance.StartBattle(PlayerField, EnemyField);
    }

    public void SetState(GameState state)
    {
        CurrentState = state;
        OnStateChanged?.Invoke(state);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetState(GameState.Win);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetState(GameState.Lose);
    }
#endif
}
