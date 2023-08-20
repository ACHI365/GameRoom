using GameRoom.Model;
using Microsoft.EntityFrameworkCore;

namespace GameRoom.Data;

public class DataContext : DbContext
{
    public DbSet<GameSession> GameSessions { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
}