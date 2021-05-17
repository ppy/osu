// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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

        private readonly string endpoint;

        [CanBeNull]
        private IHubClientConnector connector;

        private readonly IBindable<bool> isConnected = new BindableBool();

        private HubConnection connection => connector?.CurrentConnection;

        private readonly List<int> watchingUsers = new List<int>();

        private readonly object userLock = new object();

        public IBindableList<int> PlayingUsers => playingUsers;

        private readonly BindableList<int> playingUsers = new BindableList<int>();

        private readonly Dictionary<int, SpectatorState> playingUserStates = new Dictionary<int, SpectatorState>();

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
        /// Called whenever a user starts a play session, or immediately if the user is being watched and currently in a play session.
        /// </summary>
        public event Action<int, SpectatorState> OnUserBeganPlaying;

        /// <summary>
        /// Called whenever a user finishes a play session.
        /// </summary>
        public event Action<int, SpectatorState> OnUserFinishedPlaying;

        public SpectatorStreamingClient(EndpointConfiguration endpoints)
        {
            endpoint = endpoints.SpectatorEndpointUrl;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            connector = api.GetHubConnector(nameof(SpectatorStreamingClient), endpoint);

            if (connector != null)
            {
                connector.ConfigureConnection = connection =>
                {
                    // until strong typed client support is added, each method must be manually bound
                    // (see https://github.com/dotnet/aspnetcore/issues/15198)
                    connection.On<int, SpectatorState>(nameof(ISpectatorClient.UserBeganPlaying), ((ISpectatorClient)this).UserBeganPlaying);
                    connection.On<int, FrameDataBundle>(nameof(ISpectatorClient.UserSentFrames), ((ISpectatorClient)this).UserSentFrames);
                    connection.On<int, SpectatorState>(nameof(ISpectatorClient.UserFinishedPlaying), ((ISpectatorClient)this).UserFinishedPlaying);
                };

                isConnected.BindTo(connector.IsConnected);
                isConnected.BindValueChanged(connected =>
                {
                    if (connected.NewValue)
                    {
                        // get all the users that were previously being watched
                        int[] users;

                        lock (userLock)
                        {
                            users = watchingUsers.ToArray();
                            watchingUsers.Clear();
                        }

                        // resubscribe to watched users.
                        foreach (var userId in users)
                            WatchUser(userId);

                        // re-send state in case it wasn't received
                        if (isPlaying)
                            beginPlaying();
                    }
                    else
                    {
                        lock (userLock)
                        {
                            playingUsers.Clear();
                            playingUserStates.Clear();
                        }
                    }
                }, true);
            }
        }

        Task ISpectatorClient.UserBeganPlaying(int userId, SpectatorState state)
        {
            lock (userLock)
            {
                if (!playingUsers.Contains(userId))
                    playingUsers.Add(userId);

                // UserBeganPlaying() is called by the server regardless of whether the local user is watching the remote user, and is called a further time when the remote user is watched.
                // This may be a temporary thing (see: https://github.com/ppy/osu-server-spectator/blob/2273778e02cfdb4a9c6a934f2a46a8459cb5d29c/osu.Server.Spectator/Hubs/SpectatorHub.cs#L28-L29).
                // We don't want the user states to update unless the player is being watched, otherwise calling BindUserBeganPlaying() can lead to double invocations.
                if (watchingUsers.Contains(userId))
                    playingUserStates[userId] = state;
            }

            OnUserBeganPlaying?.Invoke(userId, state);

            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserFinishedPlaying(int userId, SpectatorState state)
        {
            lock (userLock)
            {
                playingUsers.Remove(userId);
                playingUserStates.Remove(userId);
            }

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

            if (!isConnected.Value) return;

            connection.SendAsync(nameof(ISpectatorServer.BeginPlaySession), currentState);
        }

        public void SendFrames(FrameDataBundle data)
        {
            if (!isConnected.Value) return;

            lastSend = connection.SendAsync(nameof(ISpectatorServer.SendFrameData), data);
        }

        public void EndPlaying()
        {
            isPlaying = false;
            currentBeatmap = null;

            if (!isConnected.Value) return;

            connection.SendAsync(nameof(ISpectatorServer.EndPlaySession), currentState);
        }

        public virtual void WatchUser(int userId)
        {
            lock (userLock)
            {
                if (watchingUsers.Contains(userId))
                    return;

                watchingUsers.Add(userId);

                if (!isConnected.Value)
                    return;
            }

            connection.SendAsync(nameof(ISpectatorServer.StartWatchingUser), userId);
        }

        public virtual void StopWatchingUser(int userId)
        {
            lock (userLock)
            {
                watchingUsers.Remove(userId);

                if (!isConnected.Value)
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

        /// <summary>
        /// Attempts to retrieve the <see cref="SpectatorState"/> for a currently-playing user.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="state">The current <see cref="SpectatorState"/> for the user, if they're playing. <c>null</c> if the user is not playing.</param>
        /// <returns><c>true</c> if successful (the user is playing), <c>false</c> otherwise.</returns>
        public bool TryGetPlayingUserState(int userId, out SpectatorState state)
        {
            lock (userLock)
                return playingUserStates.TryGetValue(userId, out state);
        }

        /// <summary>
        /// Bind an action to <see cref="OnUserBeganPlaying"/> with the option of running the bound action once immediately.
        /// </summary>
        /// <param name="callback">The action to perform when a user begins playing.</param>
        /// <param name="runOnceImmediately">Whether the action provided in <paramref name="callback"/> should be run once immediately for all users currently playing.</param>
        public void BindUserBeganPlaying(Action<int, SpectatorState> callback, bool runOnceImmediately = false)
        {
            // The lock is taken before the event is subscribed to to prevent doubling of events.
            lock (userLock)
            {
                OnUserBeganPlaying += callback;

                if (!runOnceImmediately)
                    return;

                foreach (var (userId, state) in playingUserStates)
                    callback(userId, state);
            }
        }
    }
}
