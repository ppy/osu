// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Online.Spectator
{
    public abstract class SpectatorClient : Component, ISpectatorClient
    {
        /// <summary>
        /// The maximum milliseconds between frame bundle sends.
        /// </summary>
        public const double TIME_BETWEEN_SENDS = 200;

        /// <summary>
        /// Whether the <see cref="SpectatorClient"/> is currently connected.
        /// This is NOT thread safe and usage should be scheduled.
        /// </summary>
        public abstract IBindable<bool> IsConnected { get; }

        private readonly List<int> watchingUsers = new List<int>();

        public IBindableList<int> PlayingUsers => playingUsers;
        private readonly BindableList<int> playingUsers = new BindableList<int>();

        public IBindableDictionary<int, SpectatorState> PlayingUserStates => playingUserStates;
        private readonly BindableDictionary<int, SpectatorState> playingUserStates = new BindableDictionary<int, SpectatorState>();

        private IBeatmap? currentBeatmap;
        private Score? currentScore;

        private readonly SpectatorState currentState = new SpectatorState();

        /// <summary>
        /// Whether the local user is playing.
        /// </summary>
        protected bool IsPlaying { get; private set; }

        /// <summary>
        /// Called whenever new frames arrive from the server.
        /// </summary>
        public event Action<int, FrameDataBundle>? OnNewFrames;

        /// <summary>
        /// Called whenever a user starts a play session, or immediately if the user is being watched and currently in a play session.
        /// </summary>
        public event Action<int, SpectatorState>? OnUserBeganPlaying;

        /// <summary>
        /// Called whenever a user finishes a play session.
        /// </summary>
        public event Action<int, SpectatorState>? OnUserFinishedPlaying;

        [BackgroundDependencyLoader]
        private void load()
        {
            IsConnected.BindValueChanged(connected => Schedule(() =>
            {
                if (connected.NewValue)
                {
                    // get all the users that were previously being watched
                    int[] users = watchingUsers.ToArray();
                    watchingUsers.Clear();

                    // resubscribe to watched users.
                    foreach (int userId in users)
                        WatchUser(userId);

                    // re-send state in case it wasn't received
                    if (IsPlaying)
                        BeginPlayingInternal(currentState);
                }
                else
                {
                    playingUsers.Clear();
                    playingUserStates.Clear();
                }
            }), true);
        }

        Task ISpectatorClient.UserBeganPlaying(int userId, SpectatorState state)
        {
            Schedule(() =>
            {
                if (!playingUsers.Contains(userId))
                    playingUsers.Add(userId);

                // UserBeganPlaying() is called by the server regardless of whether the local user is watching the remote user, and is called a further time when the remote user is watched.
                // This may be a temporary thing (see: https://github.com/ppy/osu-server-spectator/blob/2273778e02cfdb4a9c6a934f2a46a8459cb5d29c/osu.Server.Spectator/Hubs/SpectatorHub.cs#L28-L29).
                // We don't want the user states to update unless the player is being watched, otherwise calling BindUserBeganPlaying() can lead to double invocations.
                if (watchingUsers.Contains(userId))
                    playingUserStates[userId] = state;

                OnUserBeganPlaying?.Invoke(userId, state);
            });

            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserFinishedPlaying(int userId, SpectatorState state)
        {
            Schedule(() =>
            {
                playingUsers.Remove(userId);
                playingUserStates.Remove(userId);

                OnUserFinishedPlaying?.Invoke(userId, state);
            });

            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserSentFrames(int userId, FrameDataBundle data)
        {
            Schedule(() => OnNewFrames?.Invoke(userId, data));

            return Task.CompletedTask;
        }

        public void BeginPlaying(GameplayState state, Score score)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            if (IsPlaying)
                throw new InvalidOperationException($"Cannot invoke {nameof(BeginPlaying)} when already playing");

            IsPlaying = true;

            // transfer state at point of beginning play
            currentState.BeatmapID = score.ScoreInfo.BeatmapInfo.OnlineID;
            currentState.RulesetID = score.ScoreInfo.RulesetID;
            currentState.Mods = score.ScoreInfo.Mods.Select(m => new APIMod(m)).ToArray();

            currentBeatmap = state.Beatmap;
            currentScore = score;

            BeginPlayingInternal(currentState);
        }

        public void SendFrames(FrameDataBundle data) => lastSend = SendFramesInternal(data);

        public void EndPlaying()
        {
            // This method is most commonly called via Dispose(), which is asynchronous.
            // Todo: This should not be a thing, but requires framework changes.
            Schedule(() =>
            {
                if (!IsPlaying)
                    return;

                IsPlaying = false;
                currentBeatmap = null;

                EndPlayingInternal(currentState);
            });
        }

        public void WatchUser(int userId)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            if (watchingUsers.Contains(userId))
                return;

            watchingUsers.Add(userId);

            WatchUserInternal(userId);
        }

        public void StopWatchingUser(int userId)
        {
            // This method is most commonly called via Dispose(), which is asynchronous.
            // Todo: This should not be a thing, but requires framework changes.
            Schedule(() =>
            {
                watchingUsers.Remove(userId);
                playingUserStates.Remove(userId);
                StopWatchingUserInternal(userId);
            });
        }

        protected abstract Task BeginPlayingInternal(SpectatorState state);

        protected abstract Task SendFramesInternal(FrameDataBundle data);

        protected abstract Task EndPlayingInternal(SpectatorState state);

        protected abstract Task WatchUserInternal(int userId);

        protected abstract Task StopWatchingUserInternal(int userId);

        private readonly Queue<LegacyReplayFrame> pendingFrames = new Queue<LegacyReplayFrame>();

        private double lastSendTime;

        private Task? lastSend;

        private const int max_pending_frames = 30;

        protected override void Update()
        {
            base.Update();

            if (pendingFrames.Count > 0 && Time.Current - lastSendTime > TIME_BETWEEN_SENDS)
                purgePendingFrames();
        }

        public void HandleFrame(ReplayFrame frame)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            if (!IsPlaying)
                return;

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
