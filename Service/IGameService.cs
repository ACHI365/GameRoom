public interface IGameService
{
    Task<Result<GameSession>> CreateGameSessionAsync(int gameId, string name);
    Task<Result<GameSession>> JoinGameSessionAsync(string sessionName);
    Task<Result> LeaveGameSessionAsync(string sessionName);
}