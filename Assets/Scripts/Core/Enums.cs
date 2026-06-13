public enum Owner
{
    Player,
    Enemy
}

public enum GameState
{
    Init,
    PlayerTurn,
    PlayerSelectCard,
    PlayerSelectTarget,
    ApplyEffect,
    EnemyTurn,
    CheckResult,
    Win,
    Lose
}

public enum GameResult
{
    None,
    Win,
    Lose
}
