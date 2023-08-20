using GameRoom.Data;
using GameRoom.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GameRoom.Service;

public class GameService : IGameService
{
    private readonly DataContext _dbContext;

    public GameService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GameSession>> CreateGameSessionAsync(int gameId, string name)
    {
        var gameSession = new GameSession
        {
            GameId = gameId,
            Name = name,
            Vacancy = true,
            Occupancy = 1,
        };

        try
        {
            _dbContext.GameSessions.Add(gameSession);
            var res = await _dbContext.SaveChangesAsync();
            if (res == 0)
            {
                return Result<GameSession>.Error("Session with such name already exists");
            }
        }
        catch (DbUpdateException ex)
        {
            return Result<GameSession>.Error("Session with such name already exists");
        }

        return Result<GameSession>.Success(gameSession);
    }

    public async Task<Result<GameSession>> JoinGameSessionAsync(string sessionName)
    {
        var gameSession = await _dbContext.GameSessions.SingleOrDefaultAsync(session => session.Name == sessionName);

        if (gameSession == null)
        {
            return Result<GameSession>.Error("Session not found");
        }

        switch (gameSession.Occupancy)
        {
            case 1:
                gameSession.Vacancy = false;
                break;
            case >= 2:
                return Result<GameSession>.Error("Room is full");
        }

        gameSession.Occupancy++;
        await _dbContext.SaveChangesAsync();
        return Result<GameSession>.Success(gameSession);
    }

    public async Task<Result> LeaveGameSessionAsync(string sessionName)
    {
        var gameSession = await _dbContext.GameSessions.SingleOrDefaultAsync(session => session.Name == sessionName);

        Console.WriteLine(gameSession);

        if (gameSession == null)
        {
            return Result.Error("Session not found");
        }

        gameSession.Occupancy--;
        if (gameSession.Occupancy < 2)
        {
            gameSession.Vacancy = true;
        }

        if (gameSession.Occupancy <= 0)
        {
            _dbContext.GameSessions.Remove(gameSession);
        }

        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

}