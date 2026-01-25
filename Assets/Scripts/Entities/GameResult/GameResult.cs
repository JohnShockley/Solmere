public struct GameResult
{
    public bool Success { get; private set; }
    public GameErrorCode Error { get; private set; }
    public string Message { get; private set; }

    // Success factory
    public static GameResult Ok() => new() { Success = true, Error = GameErrorCode.None, Message = null };

    // Failure factory
    public static GameResult Fail(GameErrorCode error, string message = null) =>
        new()
        { Success = false, Error = error, Message = message ?? error.ToString() };

    // Implicit conversion to bool for convenience
    public static implicit operator bool(GameResult result) => result.Success;
}

public enum GameErrorCode
{
    None,
    OutOfResources,
    OutOfRange,
    InvalidAction,
    NotEnoughLevel,
    Unknown
}