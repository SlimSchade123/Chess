namespace Chess.Web.Hubs;

using Chess.Common.Enums;
using Chess.Data.Common.Repositories;
using Chess.Data.Models;
using Chess.Services.Data.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class GameHub
{

    private string GenerateChess960FEN()
    {
        string pieces = "RNBQKBNR";
        string backRank;
        Random rng = new ();

        do
        {
            backRank = new string(pieces.OrderBy(x => rng.Next()).ToArray());

            int bishop1 = backRank.IndexOf('B');
            int bishop2 = backRank.LastIndexOf('B');
            int king = backRank.IndexOf('K');
            int rook1 = backRank.IndexOf('R');
            int rook2 = backRank.LastIndexOf('R');

            bool bishopsOk = (bishop1 % 2) != (bishop2 % 2);
            bool kingOk = king > rook1 && king < rook2;

            if (bishopsOk && kingOk)
            {
                break;
            }
        }
        while (true);

        return backRank.ToLower() + "/pppppppp/8/8/8/8/PPPPPPPP/" + backRank;
    }

    public async Task<Player> CreateRoom(string name)
    {
        Player player = Factory.GetPlayer(name, this.Context.ConnectionId, this.Context.UserIdentifier);
        player.Rating = this.GetUserRating(player);
        this.players[player.Id] = player;
        player.Chess960Fen = this.GenerateChess960FEN();
        await this.LobbySendInternalMessage(player.Name);
        await this.Clients.All.SendAsync("AddRoom", player);

        this.waitingPlayers.Add(player);
        return player;
    }

    public async Task<Player> JoinRoom(string name, string id)
    {
        Player player2 = Factory.GetPlayer(name, this.Context.ConnectionId, this.Context.UserIdentifier);
        player2.Rating = this.GetUserRating(player2);
        this.players[player2.Id] = player2;
        var player1 = this.players[id];

        await this.StartGame(player1, player2);
        return player2;
    }

    private async Task StartGame(Player player1, Player player2)
    {
        player1.Color = Color.White;
        player2.Color = Color.Black;
        player1.HasToMove = true;

        var game = Factory.GetGame(player1, player2, this.serviceProvider);
        this.games[game.Id] = game;

        await Task.WhenAll(
            this.Groups.AddToGroupAsync(game.Player1.Id, groupName: game.Id),
            this.Groups.AddToGroupAsync(game.Player2.Id, groupName: game.Id),
            this.Clients.Group(game.Id).SendAsync("Start", game));

        await this.Clients.All.SendAsync("ListRooms", this.waitingPlayers);
        await this.GameSendInternalMessage(game.Id, player2.Name, null);

        this.waitingPlayers.Remove(player1);

        using var scope = this.serviceProvider.CreateScope();

        var gameRepository = scope.ServiceProvider.GetRequiredService<IRepository<GameEntity>>();
        await gameRepository.AddAsync(new GameEntity
        {
            Id = game.Id,
            PlayerOneName = game.Player1.Name,
            PlayerOneUserId = player1.Id,
            PlayerTwoName = game.Player2.Name,
            PlayerTwoUserId = player2.Id,
        });

        await gameRepository.SaveChangesAsync();
    }
}