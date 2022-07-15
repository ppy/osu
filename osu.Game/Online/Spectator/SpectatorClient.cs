// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Logging;
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

        /// <summary>
        /// The states of all users currently being watched.
        /// </summary>
        public virtual IBindableDictionary<int, SpectatorState> WatchedUserStates => watchedUserStates;

        /// <summary>
        /// A global list of all players currently playing.
        /// </summary>
        public IBindableList<int> PlayingUsers => playingUsers;

        /// <summary>
        /// Whether the local user is playing.
        /// </summary>
        protected bool IsPlaying { get; private set; }

        /// <summary>
        /// Called whenever new frames arrive from the server.
        /// </summary>
        public virtual event Action<int, FrameDataBundle>? OnNewFrames;

        /// <summary>
        /// Called whenever a user starts a play session, or immediately if the user is being watched and currently in a play session.
        /// </summary>
        public virtual event Action<int, SpectatorState>? OnUserBeganPlaying;

        /// <summary>
        /// Called whenever a user finishes a play session.
        /// </summary>
        public virtual event Action<int, SpectatorState>? OnUserFinishedPlaying;

        /// <summary>
        /// All users currently being watched.
        /// </summary>
        private readonly List<int> watchedUsers = new List<int>();

        private readonly BindableDictionary<int, SpectatorState> watchedUserStates = new BindableDictionary<int, SpectatorState>();
        private readonly BindableList<int> playingUsers = new BindableList<int>();
        private readonly SpectatorState currentState = new SpectatorState();

        private IBeatmap? currentBeatmap;
        private Score? currentScore;

        private readonly Queue<FrameDataBundle> pendingFrameBundles = new Queue<FrameDataBundle>();

        private readonly Queue<LegacyReplayFrame> pendingFrames = new Queue<LegacyReplayFrame>();

        private double lastPurgeTime;

        private Task? lastSend;

        private const int max_pending_frames = 30;

        [BackgroundDependencyLoader]
        private void load()
        {
            IsConnected.BindValueChanged(connected => Schedule(() =>
            {
                if (connected.NewValue)
                {
                    // get all the users that were previously being watched
                    int[] users = watchedUsers.ToArray();
                    watchedUsers.Clear();

                    // resubscribe to watched users.
                    foreach (int userId in users)
                        WatchUser(userId);

                    // re-send state in case it wasn't received
                    if (IsPlaying)
                        // TODO: this is likely sent out of order after a reconnect scenario. needs further consideration.
                        BeginPlayingInternal(currentState);
                }
                else
                {
                    playingUsers.Clear();
                    watchedUserStates.Clear();
                }
            }), true);
        }

        Task ISpectatorClient.UserBeganPlaying(int userId, SpectatorState state)
        {
            Schedule(() =>
            {
                if (!playingUsers.Contains(userId))
                    playingUsers.Add(userId);

                if (watchedUsers.Contains(userId))
                    watchedUserStates[userId] = state;

                OnUserBeganPlaying?.Invoke(userId, state);
            });

            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserFinishedPlaying(int userId, SpectatorState state)
        {
            Schedule(() =>
            {
                playingUsers.Remove(userId);

                if (watchedUsers.Contains(userId))
                    watchedUserStates[userId] = state;

                OnUserFinishedPlaying?.Invoke(userId, state);
            });

            return Task.CompletedTask;
        }

        Task ISpectatorClient.UserSentFrames(int userId, FrameDataBundle data)
        {
            if (data.Frames.Count > 0)
                data.Frames[^1].Header = data.Header;

            Schedule(() => OnNewFrames?.Invoke(userId, data));

            return Task.CompletedTask;
        }

        public void BeginPlaying(GameplayState state, Score score)
        {
            // This schedule is only here to match the one below in `EndPlaying`.
            Schedule(() =>
            {
                if (IsPlaying)
                    throw new InvalidOperationException($"Cannot invoke {nameof(BeginPlaying)} when already playing");

                IsPlaying = true;

                // transfer state at point of beginning play
                currentState.BeatmapID = score.ScoreInfo.BeatmapInfo.OnlineID;
                currentState.RulesetID = score.ScoreInfo.RulesetID;
                currentState.Mods = score.ScoreInfo.Mods.Select(m => new APIMod(m)).ToArray();
                currentState.State = SpectatedUserState.Playing;
                currentState.MaximumScoringValues = state.ScoreProcessor.MaximumScoringValues;

                currentBeatmap = state.Beatmap;
                currentScore = score;

                BeginPlayingInternal(currentState);
            });
        }

        public void HandleFrame(ReplayFrame frame) => Schedule(() =>
        {
            if (!IsPlaying)
            {
                Logger.Log($"Frames arrived at {nameof(SpectatorClient)} outside of gameplay scope and will be ignored.");
                return;
            }

            if (frame is IConvertibleReplayFrame convertible)
            {
                Debug.Assert(currentBeatmap != null);
                pendingFrames.Enqueue(convertible.ToLegacy(currentBeatmap));
            }

            if (pendingFrames.Count > max_pending_frames)
                purgePendingFrames();
        });

        public void EndPlaying(GameplayState state)
        {
            // This method is most commonly called via Dispose(), which is can be asynchronous (via the AsyncDisposalQueue).
            // We probably need to find a better way to handle this...
            Schedule(() =>
            {
                if (!IsPlaying)
                    return;

                if (pendingFrames.Count > 0)
                    purgePendingFrames();

                IsPlaying = false;
                currentBeatmap = null;

                if (state.HasPassed)
                    currentState.State = SpectatedUserState.Passed;
                else if (state.HasFailed)
                    currentState.State = SpectatedUserState.Failed;
                else
                    currentState.State = SpectatedUserState.Quit;

                EndPlayingInternal(currentState);
            });
        }

        public virtual void WatchUser(int userId)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            if (watchedUsers.Contains(userId))
                return;

            watchedUsers.Add(userId);

            WatchUserInternal(userId);
        }

        public void StopWatchingUser(int userId)
        {
            // This method is most commonly called via Dispose(), which is asynchronous.
            // Todo: This should not be a thing, but requires framework changes.
            Schedule(() =>
            {
                watchedUsers.Remove(userId);
                watchedUserStates.Remove(userId);
                StopWatchingUserInternal(userId);
            });
        }

        protected abstract Task BeginPlayingInternal(SpectatorState state);

        protected abstract Task SendFramesInternal(FrameDataBundle bundle);

        protected abstract Task EndPlayingInternal(SpectatorState state);

        protected abstract Task WatchUserInternal(int userId);

        protected abstract Task StopWatchingUserInternal(int userId);

        protected override void Update()
        {
            base.Update();

            if (pendingFrames.Count > 0 && Time.Current - lastPurgeTime > TIME_BETWEEN_SENDS)
                purgePendingFrames();
        }

        private void purgePendingFrames()
        {
            if (pendingFrames.Count == 0)
                return;

            Debug.Assert(currentScore != null);

            var frames = pendingFrames.ToArray();
            var bundle = new FrameDataBundle(currentScore.ScoreInfo, frames);

            pendingFrames.Clear();
            lastPurgeTime = Time.Current;

            pendingFrameBundles.Enqueue(bundle);

            sendNextBundleIfRequired();
        }

        private void sendNextBundleIfRequired()
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            if (lastSend?.IsCompleted == false)
                return;

            if (!pendingFrameBundles.TryPeek(out var bundle))
                return;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            lastSend = tcs.Task;

            SendFramesInternal(bundle).ContinueWith(t =>
            {
                // Handle exception outside of `Schedule` to ensure it doesn't go unovserved.
                bool wasSuccessful = t.Exception == null;

                return Schedule(() =>
                {
                    // If the last bundle send wasn't successful, try again without dequeuing.
                    if (wasSuccessful)
                        pendingFrameBundles.Dequeue();

                    tcs.SetResult(wasSuccessful);
                    sendNextBundleIfRequired();
                });
            });
        }
    }
}
