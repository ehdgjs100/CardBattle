using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<CardDataBase> playerDeck;

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
        StartCoroutine(InitBattle());
    }

    private IEnumerator InitBattle()
    {
        yield return null;

        List<CardDataBase> playerCards;
        List<CardDataBase> enemyCards;

        if (TutorialManager.IsTutorialMode())
        {
            playerCards = TutorialManager.Instance.PlayerDeck;
            enemyCards = TutorialManager.Instance.EnemyDeck;
        }
        else
        {
            playerCards = (CardManager.Instance != null && CardManager.Instance.PlayerDeck.Count > 0)
                ? CardManager.Instance.GetBattleDeck()
                : playerDeck;
            enemyCards = BuildEnemyDeck(10);
        }

        PlayerField = new CardField(playerCards, Owner.Player);
        EnemyField = new CardField(enemyCards, Owner.Enemy);

        TurnManager.Instance.StartBattle(PlayerField, EnemyField);
    }

    private List<CardDataBase> BuildEnemyDeck(int count)
    {
        var result = new List<CardDataBase>();
        if (CardManager.Instance == null) return result;

        for (int i = 0; i < count; i++)
        {
            CardDataBase card = CardManager.Instance.DrawRandom();
            if (card != null)
                result.Add(Instantiate(card));
        }
        return result;
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
