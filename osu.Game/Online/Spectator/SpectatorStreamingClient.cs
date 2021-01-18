// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Online.Spectator
{
    public class SpectatorStreamingClient : Component, ISpectatorClient
    {
        /// <summary>
        /// The maximum milliseconds between frame bundle sends.
        /// </summary>
        public const double TIME_BETWEEN_SENDS = 200;

        private HubConnection connection;

        private readonly List<int> watchingUsers = new List<int>();

        private readonly object userLock = new object();

        public IBindableList<int> PlayingUsers => playingUsers;

        private readonly BindableList<int> playingUsers = new BindableList<int>();

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        private bool isConnected;

        [Resolved]
        private IAPIProvider api { get; set; }

        [CanBeNull]
        private IBeatmap currentBeatmap;

        [CanBeNull]
        private Score currentScore;

        [Resolved]
        private IBindable<RulesetInfo> currentRuleset { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> currentMods { get; set; }

        private readonly SpectatorState currentState = new SpectatorState();

        private bool isPlaying;

        /// <summary>
        /// Called whenever new frames arrive from the server.
        /// </summary>
        public event Action<int, FrameDataBundle> OnNewFrames;

        /// <summary>
        /// Called whenever a user starts a play session.
        /// </summary>
        public event Action<int, SpectatorState> OnUserBeganPlaying;

        /// <summary>
        /// Called whenever a user finishes a play session.
        /// </summary>
        public event Action<int, SpectatorState> OnUserFinishedPlaying;

        private readonly string endpoint;

        public SpectatorStreamingClient(EndpointConfiguration endpoints)
        {
            endpoint = endpoints.SpectatorEndpointUrl;
        }

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
                    Task.Run(Connect);
                    break;
            }
        }

        protected virtual async Task Connect()
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

                if (ex != null)
                {
                    Logger.Log($"Spectator client lost connection: {ex}", LoggingTarget.Network);
                    await tryUntilConnected();
                }
            };

            await tryUntilConnected();

            async Task tryUntilConnected()
            {
                Logger.Log("Spectator client connecting...", LoggingTarget.Network);

                while (api.State.Value == APIState.Online)
                {
                    try
                    {
                        // reconnect on any failure
                        await connection.StartAsync();
                        Logger.Log("Spectator client connected!", LoggingTarget.Network);

                        // get all the users that were previously being watched
                        int[] users;

                        lock (userLock)
                        {
                            users = watchingUsers.ToArray();
                            watchingUsers.Clear();
                        }

                        // success
                        isConnected = true;

                        // resubscribe to watched users
                        foreach (var userId in users)
                            WatchUser(userId);

                        // re-send state in case it wasn't received
                        if (isPlaying)
                            beginPlaying();

                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Spectator client connection error: {e}", LoggingTarget.Network);
                        await Task.Delay(5000);
                    }
                }
            }
        }

        Task ISpectatorClient.UserBeganPlaying(int userId, SpectatorState state)
        {
            if (!playingUsers.Contains(userId))
                playingUsers.Add(userId);

            OnUserBeganPlaying?.Invoke(userId, state);

            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserFinishedPlaying(int userId, SpectatorState state)
        {
            playingUsers.Remove(userId);

            OnUserFinishedPlaying?.Invoke(userId, state);

            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserSentFrames(int userId, FrameDataBundle data)
        {
            OnNewFrames?.Invoke(userId, data);

            return Task.CompletedTask;
        }

        public void BeginPlaying(GameplayBeatmap beatmap, Score score)
        {
            if (isPlaying)
                throw new InvalidOperationException($"Cannot invoke {nameof(BeginPlaying)} when already playing");

            isPlaying = true;

            // transfer state at point of beginning play
            currentState.BeatmapID = beatmap.BeatmapInfo.OnlineBeatmapID;
            currentState.RulesetID = currentRuleset.Value.ID;
            currentState.Mods = currentMods.Value.Select(m => new APIMod(m));

            currentBeatmap = beatmap.PlayableBeatmap;
            currentScore = score;

            beginPlaying();
        }

        private void beginPlaying()
        {
            Debug.Assert(isPlaying);

            if (!isConnected) return;

            connection.SendAsync(nameof(ISpectatorServer.BeginPlaySession), currentState);
        }

        public void SendFrames(FrameDataBundle data)
        {
            if (!isConnected) return;

            lastSend = connection.SendAsync(nameof(ISpectatorServer.SendFrameData), data);
        }

        public void EndPlaying()
        {
            isPlaying = false;
            currentBeatmap = null;

            if (!isConnected) return;

            connection.SendAsync(nameof(ISpectatorServer.EndPlaySession), currentState);
        }

        public virtual void WatchUser(int userId)
        {
            lock (userLock)
            {
                if (watchingUsers.Contains(userId))
                    return;

                watchingUsers.Add(userId);

                if (!isConnected)
                    return;
            }

            connection.SendAsync(nameof(ISpectatorServer.StartWatchingUser), userId);
        }

        public void StopWatchingUser(int userId)
        {
            lock (userLock)
            {
                watchingUsers.Remove(userId);

                if (!isConnected)
                    return;
            }

            connection.SendAsync(nameof(ISpectatorServer.EndWatchingUser), userId);
        }

        private readonly Queue<LegacyReplayFrame> pendingFrames = new Queue<LegacyReplayFrame>();

        private double lastSendTime;

        private Task lastSend;

        private const int max_pending_frames = 30;

        protected override void Update()
        {
            base.Update();

            if (pendingFrames.Count > 0 && Time.Current - lastSendTime > TIME_BETWEEN_SENDS)
                purgePendingFrames();
        }

        public void HandleFrame(ReplayFrame frame)
        {
            if (frame is IConvertibleReplayFrame convertible)
                pendingFrames.Enqueue(convertible.ToLegacy(currentBeatmap));

            if (pendingFrames.Count > max_pending_frames)
                purgePendingFrames();
        }

        private void purgePendingFrames()
        {
            if (lastSend?.IsCompleted == false)
                return;

            var frames = pendingFrames.ToArray();

            pendingFrames.Clear();

            Debug.Assert(currentScore != null);

            SendFrames(new FrameDataBundle(currentScore.ScoreInfo, frames));

            lastSendTime = Time.Current;
        }
    }
}
