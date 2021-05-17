// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Spectator;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Screens.Spectate
{
    /// <summary>
    /// A <see cref="OsuScreen"/> which spectates one or more users.
    /// </summary>
    public abstract class SpectatorScreen : OsuScreen
    {
        protected IReadOnlyList<int> UserIds => userIds;

        private readonly List<int> userIds = new List<int>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private SpectatorStreamingClient spectatorClient { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        // A lock is used to synchronise access to spectator/gameplay states, since this class is a screen which may become non-current and stop receiving updates at any point.
        private readonly object stateLock = new object();

        private readonly Dictionary<int, User> userMap = new Dictionary<int, User>();
        private readonly Dictionary<int, GameplayState> gameplayStates = new Dictionary<int, GameplayState>();

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;

        /// <summary>
        /// Creates a new <see cref="SpectatorScreen"/>.
        /// </summary>
        /// <param name="userIds">The users to spectate.</param>
        protected SpectatorScreen(params int[] userIds)
        {
            this.userIds.AddRange(userIds);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            populateAllUsers().ContinueWith(_ => Schedule(() =>
            {
                spectatorClient.BindUserBeganPlaying(userBeganPlaying, true);
                spectatorClient.OnUserFinishedPlaying += userFinishedPlaying;
                spectatorClient.OnNewFrames += userSentFrames;

                managerUpdated = beatmaps.ItemUpdated.GetBoundCopy();
                managerUpdated.BindValueChanged(beatmapUpdated);

                lock (stateLock)
                {
                    foreach (var (id, _) in userMap)
                        spectatorClient.WatchUser(id);
                }
            }));
        }

        private Task populateAllUsers()
        {
            var userLookupTasks = new List<Task>();

            foreach (var u in userIds)
            {
                userLookupTasks.Add(userLookupCache.GetUserAsync(u).ContinueWith(task =>
                {
                    if (!task.IsCompletedSuccessfully)
                        return;

                    lock (stateLock)
                        userMap[u] = task.Result;
                }));
            }

            return Task.WhenAll(userLookupTasks);
        }

        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> e)
        {
            if (!e.NewValue.TryGetTarget(out var beatmapSet))
                return;

            lock (stateLock)
            {
                foreach (var (userId, _) in userMap)
                {
                    if (!spectatorClient.TryGetPlayingUserState(userId, out var userState))
                        continue;

                    if (beatmapSet.Beatmaps.Any(b => b.OnlineBeatmapID == userState.BeatmapID))
                        updateGameplayState(userId);
                }
            }
        }

        private void userBeganPlaying(int userId, SpectatorState state)
        {
            if (state.RulesetID == null || state.BeatmapID == null)
                return;

            lock (stateLock)
            {
                if (!userMap.ContainsKey(userId))
                    return;

                // The user may have stopped playing.
                if (!spectatorClient.TryGetPlayingUserState(userId, out _))
                    return;

                Schedule(() => OnUserStateChanged(userId, state));

                updateGameplayState(userId);
            }
        }

        private void updateGameplayState(int userId)
        {
            lock (stateLock)
            {
                Debug.Assert(userMap.ContainsKey(userId));

                // The user may have stopped playing.
                if (!spectatorClient.TryGetPlayingUserState(userId, out var spectatorState))
                    return;

                var user = userMap[userId];

                var resolvedRuleset = rulesets.AvailableRulesets.FirstOrDefault(r => r.ID == spectatorState.RulesetID)?.CreateInstance();
                if (resolvedRuleset == null)
                    return;

                var resolvedBeatmap = beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == spectatorState.BeatmapID);
                if (resolvedBeatmap == null)
                    return;

                var score = new Score
                {
                    ScoreInfo = new ScoreInfo
                    {
                        Beatmap = resolvedBeatmap,
                        User = user,
                        Mods = spectatorState.Mods.Select(m => m.ToMod(resolvedRuleset)).ToArray(),
                        Ruleset = resolvedRuleset.RulesetInfo,
                    },
                    Replay = new Replay { HasReceivedAllFrames = false },
                };

                var gameplayState = new GameplayState(score, resolvedRuleset, beatmaps.GetWorkingBeatmap(resolvedBeatmap));

                gameplayStates[userId] = gameplayState;
                Schedule(() => StartGameplay(userId, gameplayState));
            }
        }

        private void userSentFrames(int userId, FrameDataBundle bundle)
        {
            lock (stateLock)
            {
                if (!userMap.ContainsKey(userId))
                    return;

                if (!gameplayStates.TryGetValue(userId, out var gameplayState))
                    return;

                // The ruleset instance should be guaranteed to be in sync with the score via ScoreLock.
                Debug.Assert(gameplayState.Ruleset != null && gameplayState.Ruleset.RulesetInfo.Equals(gameplayState.Score.ScoreInfo.Ruleset));

                foreach (var frame in bundle.Frames)
                {
                    IConvertibleReplayFrame convertibleFrame = gameplayState.Ruleset.CreateConvertibleReplayFrame();
                    convertibleFrame.FromLegacy(frame, gameplayState.Beatmap.Beatmap);

                    var convertedFrame = (ReplayFrame)convertibleFrame;
                    convertedFrame.Time = frame.Time;

                    gameplayState.Score.Replay.Frames.Add(convertedFrame);
                }
            }
        }

        private void userFinishedPlaying(int userId, SpectatorState state)
        {
            lock (stateLock)
            {
                if (!userMap.ContainsKey(userId))
                    return;

                if (!gameplayStates.TryGetValue(userId, out var gameplayState))
                    return;

                gameplayState.Score.Replay.HasReceivedAllFrames = true;

                gameplayStates.Remove(userId);
                Schedule(() => EndGameplay(userId));
            }
        }

        /// <summary>
        /// Invoked when a spectated user's state has changed.
        /// </summary>
        /// <param name="userId">The user whose state has changed.</param>
        /// <param name="spectatorState">The new state.</param>
        protected abstract void OnUserStateChanged(int userId, [NotNull] SpectatorState spectatorState);

        /// <summary>
        /// Starts gameplay for a user.
        /// </summary>
        /// <param name="userId">The user to start gameplay for.</param>
        /// <param name="gameplayState">The gameplay state.</param>
        protected abstract void StartGameplay(int userId, [NotNull] GameplayState gameplayState);

        /// <summary>
        /// Ends gameplay for a user.
        /// </summary>
        /// <param name="userId">The user to end gameplay for.</param>
        protected abstract void EndGameplay(int userId);

        /// <summary>
        /// Stops spectating a user.
        /// </summary>
        /// <param name="userId">The user to stop spectating.</param>
        protected void RemoveUser(int userId)
        {
            lock (stateLock)
            {
                userFinishedPlaying(userId, null);

                userIds.Remove(userId);
                userMap.Remove(userId);

                spectatorClient.StopWatchingUser(userId);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient != null)
            {
                spectatorClient.OnUserBeganPlaying -= userBeganPlaying;
                spectatorClient.OnUserFinishedPlaying -= userFinishedPlaying;
                spectatorClient.OnNewFrames -= userSentFrames;

                lock (stateLock)
                {
                    foreach (var (userId, _) in userMap)
                        spectatorClient.StopWatchingUser(userId);
                }
            }

            managerUpdated?.UnbindAll();
        }
    }
}
