namespace GameRoom.Model.Dto;

public class UserConnection
{
    public string User { get; set; }
    public string SessionName { get; set; }
    public int GameId { get; set; }
    public string UniqueId { get; set; }
}