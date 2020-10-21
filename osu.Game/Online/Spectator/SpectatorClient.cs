using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Server.Spectator.Hubs;

namespace osu.Game.Online.Spectator
{
    public class SpectatorClient : ISpectatorClient
    {
        private readonly HubConnection connection;

        private readonly List<string> watchingUsers = new List<string>();

        public SpectatorClient(HubConnection connection)
        {
            this.connection = connection;

            // this is kind of SILLY
            // https://github.com/dotnet/aspnetcore/issues/15198
            connection.On<string, int>(nameof(ISpectatorClient.UserBeganPlaying), ((ISpectatorClient)this).UserBeganPlaying);
            connection.On<string, FrameDataBundle>(nameof(ISpectatorClient.UserSentFrames), ((ISpectatorClient)this).UserSentFrames);
            connection.On<string, int>(nameof(ISpectatorClient.UserFinishedPlaying), ((ISpectatorClient)this).UserFinishedPlaying);
        }

        Task ISpectatorClient.UserBeganPlaying(string userId, int beatmapId)
        {
            if (connection.ConnectionId != userId)
            {
                if (watchingUsers.Contains(userId))
                {
                    Console.WriteLine($"{connection.ConnectionId} received began playing for already watched user {userId}");
                }
                else
                {
                    Console.WriteLine($"{connection.ConnectionId} requesting watch other user {userId}");
                    WatchUser(userId);
                    watchingUsers.Add(userId);
                }
            }
            else
            {
                Console.WriteLine($"{connection.ConnectionId} Received user playing event for self {beatmapId}");
            }

            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserFinishedPlaying(string userId, int beatmapId)
        {
            Console.WriteLine($"{connection.ConnectionId} Received user finished event {beatmapId}");
            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserSentFrames(string userId, FrameDataBundle data)
        {
            Console.WriteLine($"{connection.ConnectionId} Received frames from {userId}: {data.Frames.First().ToString()}");
            return Task.CompletedTask;
        }

        public Task BeginPlaying(int beatmapId) => connection.SendAsync(nameof(ISpectatorServer.BeginPlaySession), beatmapId);

        public Task SendFrames(FrameDataBundle data) => connection.SendAsync(nameof(ISpectatorServer.SendFrameData), data);

        public Task EndPlaying(int beatmapId) => connection.SendAsync(nameof(ISpectatorServer.EndPlaySession), beatmapId);

        private Task WatchUser(string userId) => connection.SendAsync(nameof(ISpectatorServer.StartWatchingUser), userId);
    }
}
