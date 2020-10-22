using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Online.Spectator
{
    public class SpectatorStreamingClient : Component, ISpectatorClient
    {
        private HubConnection connection;

        private readonly List<int> watchingUsers = new List<int>();

        public IBindableList<int> PlayingUsers => playingUsers;

        private readonly BindableList<int> playingUsers = new BindableList<int>();

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        private bool isConnected;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        private readonly SpectatorState currentState = new SpectatorState();

        private bool isPlaying;

        /// <summary>
        /// Called whenever new frames arrive from the server.
        /// </summary>
        public event Action<int, FrameDataBundle> OnNewFrames;

        [BackgroundDependencyLoader]
        private void load()
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(apiStateChanged, true);
        }

        private void apiStateChanged(ValueChangedEvent<APIState> state)
        {
            switch (state.NewValue)
            {
                case APIState.Failing:
                case APIState.Offline:
                    connection?.StopAsync();
                    connection = null;
                    break;

                case APIState.Online:
                    Task.Run(connect);
                    break;
            }
        }

#if DEBUG
        private const string endpoint = "http://localhost:5009/spectator";
#else
        private const string endpoint = "https://spectator.ppy.sh/spectator";
#endif

        private async Task connect()
        {
            if (connection != null)
                return;

            connection = new HubConnectionBuilder()
                         .WithUrl(endpoint, options =>
                         {
                             options.Headers.Add("Authorization", $"Bearer {api.AccessToken}");
                         })
                         .AddNewtonsoftJsonProtocol(options => { options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; })
                         .Build();

            // until strong typed client support is added, each method must be manually bound (see https://github.com/dotnet/aspnetcore/issues/15198)
            connection.On<int, SpectatorState>(nameof(ISpectatorClient.UserBeganPlaying), ((ISpectatorClient)this).UserBeganPlaying);
            connection.On<int, FrameDataBundle>(nameof(ISpectatorClient.UserSentFrames), ((ISpectatorClient)this).UserSentFrames);
            connection.On<int, SpectatorState>(nameof(ISpectatorClient.UserFinishedPlaying), ((ISpectatorClient)this).UserFinishedPlaying);

            connection.Closed += async ex =>
            {
                isConnected = false;
                playingUsers.Clear();

                if (ex != null) await tryUntilConnected();
            };

            await tryUntilConnected();

            async Task tryUntilConnected()
            {
                while (api.State.Value == APIState.Online)
                {
                    try
                    {
                        // reconnect on any failure
                        await connection.StartAsync();

                        // success
                        isConnected = true;

                        // resubscribe to watched users
                        var users = watchingUsers.ToArray();
                        watchingUsers.Clear();
                        foreach (var userId in users)
                            WatchUser(userId);

                        // re-send state in case it wasn't received
                        if (isPlaying)
                            beginPlaying();

                        break;
                    }
                    catch
                    {
                        await Task.Delay(5000);
                    }
                }
            }
        }

        Task ISpectatorClient.UserBeganPlaying(int userId, SpectatorState state)
        {
            if (!playingUsers.Contains(userId))
                playingUsers.Add(userId);

            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserFinishedPlaying(int userId, SpectatorState state)
        {
            playingUsers.Remove(userId);
            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserSentFrames(int userId, FrameDataBundle data)
        {
            OnNewFrames?.Invoke(userId, data);
            return Task.CompletedTask;
        }

        public void BeginPlaying()
        {
            if (!isConnected) return;

            if (isPlaying)
                throw new InvalidOperationException($"Cannot invoke {nameof(BeginPlaying)} when already playing");

            isPlaying = true;

            // transfer state at point of beginning play
            currentState.BeatmapID = beatmap.Value.BeatmapInfo.OnlineBeatmapID;
            currentState.Mods = mods.Value.Select(m => new APIMod(m));

            beginPlaying();
        }

        private void beginPlaying()
        {
            Debug.Assert(isPlaying);

            connection.SendAsync(nameof(ISpectatorServer.BeginPlaySession), currentState);
        }

        public void SendFrames(FrameDataBundle data)
        {
            if (!isConnected) return;

            connection.SendAsync(nameof(ISpectatorServer.SendFrameData), data);
        }

        public void EndPlaying()
        {
            if (!isConnected) return;

            if (!isPlaying)
                throw new InvalidOperationException($"Cannot invoke {nameof(EndPlaying)} when not playing");

            isPlaying = false;
            connection.SendAsync(nameof(ISpectatorServer.EndPlaySession), currentState);
        }

        public void WatchUser(int userId)
        {
            if (!isConnected) return;

            if (watchingUsers.Contains(userId))
                return;

            watchingUsers.Add(userId);
            connection.SendAsync(nameof(ISpectatorServer.StartWatchingUser), userId);
        }

        public void HandleFrame(ReplayFrame frame)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global (implemented by rulesets)
            if (frame is IConvertibleReplayFrame convertible)
                // TODO: don't send a bundle for each individual frame
                SendFrames(new FrameDataBundle(new[] { convertible.ToLegacy(beatmap.Value.Beatmap) }));
        }
    }
}
