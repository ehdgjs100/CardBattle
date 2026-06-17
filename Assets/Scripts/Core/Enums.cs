public enum CardType
{
    Normal,
    Ranged,
    Muso,
    Healer,
    Tanker,
    Assassin
}

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
