using GameRoom.Model;
using GameRoom.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GameRoom.GameHub;

public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private readonly string _botUser;
    private readonly IDictionary<string, UserConnection> _connections;

    public GameHub(IGameService gameService, IDictionary<string, UserConnection> connections)
    {
        _gameService = gameService;
        _botUser = "MyChat Bot";
        _connections = connections;
    }

    public async Task<IActionResult> CreateAndJoin(UserConnection userConnection)
    {
        var result = await _gameService.CreateGameSessionAsync(userConnection.GameId, userConnection.SessionName);

        var res = new ObjectResult(result.Data);

        if (result.IsSuccess)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.SessionName);

            _connections[Context.ConnectionId] = userConnection;

            await Clients.Group(userConnection.SessionName).SendAsync("ReceiveMessage", _botUser,
                $"{userConnection.User} has joined {userConnection.SessionName}");

            await SendUsersConnected(userConnection.SessionName);

            res.StatusCode = 200;
            return res;
        }

        res = new ObjectResult(result.ErrorMessage)
        {
            StatusCode = 400
        };
        return res;
    }

    public async Task<IActionResult> JoinSession(UserConnection userConnection)
    {
        var result = await _gameService.JoinGameSessionAsync(userConnection.SessionName);

        var res = new ObjectResult(result.Data);

        if (result.IsSuccess)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.SessionName);

            _connections[Context.ConnectionId] = userConnection;

            await Clients.Group(userConnection.SessionName).SendAsync("ReceiveMessage", _botUser,
                $"{userConnection.User} has joined {userConnection.SessionName}");

            await SendUsersConnected(userConnection.SessionName);

            res.StatusCode = 200;
            return res;
        }

        res = new ObjectResult(result.ErrorMessage)
        {
            StatusCode = 400
        };
        return res;
    }
        
    public async Task SendMove(TicTacData data)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
        {
            await Clients.Group(userConnection.SessionName).SendAsync("GetMove", userConnection.UniqueId, data);
        }
    }
    
    public async Task SendRestart()
    {
        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
        {
            await Clients.Group(userConnection.SessionName).SendAsync("Restart", userConnection.User);
        }
    }

    public async Task SendMessage(string message)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
        {
            await Clients.Group(userConnection.SessionName).SendAsync("ReceiveMessage", userConnection.User,  message);
        }
    }

    public async override Task OnDisconnectedAsync(Exception exception)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
        {
            await _gameService.LeaveGameSessionAsync(userConnection.SessionName);
            _connections.Remove(Context.ConnectionId);
            await Clients.Group(userConnection.SessionName)
                .SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");
            await SendUsersConnected(userConnection.SessionName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task SendUsersConnected(string room)
    {
        var users = _connections.Values
            .Where(c => c.SessionName == room)
            .Select(c => { return new string[] { c.User, c.UniqueId }; });

        return Clients.Group(room).SendAsync("UsersInRoom", users);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}