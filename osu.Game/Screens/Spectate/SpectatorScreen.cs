// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Spectator;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Spectate
{
    /// <summary>
    /// A <see cref="OsuScreen"/> which spectates one or more users.
    /// </summary>
    public abstract class SpectatorScreen : OsuScreen
    {
        protected IReadOnlyList<int> Users => users;

        private readonly List<int> users = new List<int>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        private readonly IBindableDictionary<int, SpectatorState> playingUserStates = new BindableDictionary<int, SpectatorState>();

        private readonly Dictionary<int, APIUser> userMap = new Dictionary<int, APIUser>();
        private readonly Dictionary<int, SpectatorGameplayState> gameplayStates = new Dictionary<int, SpectatorGameplayState>();

        /// <summary>
        /// Creates a new <see cref="SpectatorScreen"/>.
        /// </summary>
        /// <param name="users">The users to spectate.</param>
        protected SpectatorScreen(params int[] users)
        {
            this.users.AddRange(users);
        }

        [Resolved]
        private RealmContextFactory realmContextFactory { get; set; }

        private IDisposable realmSubscription;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userLookupCache.GetUsersAsync(users.ToArray()).ContinueWith(task => Schedule(() =>
            {
                var foundUsers = task.GetResultSafely();

                foreach (var u in foundUsers)
                {
                    if (u == null)
                        continue;

                    userMap[u.Id] = u;
                }

                playingUserStates.BindTo(spectatorClient.PlayingUserStates);
                playingUserStates.BindCollectionChanged(onPlayingUserStatesChanged, true);

                realmSubscription = realmContextFactory.Context
                                                       .All<BeatmapSetInfo>()
                                                       .Where(s => !s.DeletePending)
                                                       .QueryAsyncWithNotifications((items, changes, ___) =>
                                                       {
                                                           if (changes?.InsertedIndices == null)
                                                               return;

                                                           foreach (int c in changes.InsertedIndices)
                                                               beatmapUpdated(items[c]);
                                                       });

                foreach ((int id, var _) in userMap)
                    spectatorClient.WatchUser(id);
            }));
        }

        private void beatmapUpdated(BeatmapSetInfo beatmapSet)
        {
            foreach ((int userId, _) in userMap)
            {
                if (!playingUserStates.TryGetValue(userId, out var userState))
                    continue;

                if (beatmapSet.Beatmaps.Any(b => b.OnlineID == userState.BeatmapID))
                    updateGameplayState(userId);
            }
        }

        private void onPlayingUserStatesChanged(object sender, NotifyDictionaryChangedEventArgs<int, SpectatorState> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    foreach ((int userId, var state) in e.NewItems.AsNonNull())
                        onUserStateAdded(userId, state);
                    break;

                case NotifyDictionaryChangedAction.Remove:
                    foreach ((int userId, var _) in e.OldItems.AsNonNull())
                        onUserStateRemoved(userId);
                    break;

                case NotifyDictionaryChangedAction.Replace:
                    foreach ((int userId, var _) in e.OldItems.AsNonNull())
                        onUserStateRemoved(userId);

                    foreach ((int userId, var state) in e.NewItems.AsNonNull())
                        onUserStateAdded(userId, state);
                    break;
            }
        }

        private void onUserStateAdded(int userId, SpectatorState state)
        {
            if (state.RulesetID == null || state.BeatmapID == null)
                return;

            if (!userMap.ContainsKey(userId))
                return;

            Schedule(() => OnUserStateChanged(userId, state));
            updateGameplayState(userId);
        }

        private void onUserStateRemoved(int userId)
        {
            if (!userMap.ContainsKey(userId))
                return;

            if (!gameplayStates.TryGetValue(userId, out var gameplayState))
                return;

            gameplayState.Score.Replay.HasReceivedAllFrames = true;

            gameplayStates.Remove(userId);
            Schedule(() => EndGameplay(userId));
        }

        private void updateGameplayState(int userId)
        {
            Debug.Assert(userMap.ContainsKey(userId));

            var user = userMap[userId];
            var spectatorState = playingUserStates[userId];

            var resolvedRuleset = rulesets.AvailableRulesets.FirstOrDefault(r => r.OnlineID == spectatorState.RulesetID)?.CreateInstance();
            if (resolvedRuleset == null)
                return;

            var resolvedBeatmap = beatmaps.QueryBeatmap(b => b.OnlineID == spectatorState.BeatmapID);
            if (resolvedBeatmap == null)
                return;

            var score = new Score
            {
                ScoreInfo = new ScoreInfo
                {
                    BeatmapInfo = resolvedBeatmap,
                    User = user,
                    Mods = spectatorState.Mods.Select(m => m.ToMod(resolvedRuleset)).ToArray(),
                    Ruleset = resolvedRuleset.RulesetInfo,
                },
                Replay = new Replay { HasReceivedAllFrames = false },
            };

            var gameplayState = new SpectatorGameplayState(score, resolvedRuleset, beatmaps.GetWorkingBeatmap(resolvedBeatmap));

            gameplayStates[userId] = gameplayState;
            Schedule(() => StartGameplay(userId, gameplayState));
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
        /// <param name="spectatorGameplayState">The gameplay state.</param>
        protected abstract void StartGameplay(int userId, [NotNull] SpectatorGameplayState spectatorGameplayState);

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
            onUserStateRemoved(userId);

            users.Remove(userId);
            userMap.Remove(userId);

            spectatorClient.StopWatchingUser(userId);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient != null)
            {
                foreach ((int userId, var _) in userMap)
                    spectatorClient.StopWatchingUser(userId);
            }

            realmSubscription?.Dispose();
        }
    }
}
