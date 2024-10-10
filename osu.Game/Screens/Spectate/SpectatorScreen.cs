// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using Realms;

namespace osu.Game.Screens.Spectate
{
    /// <summary>
    /// A <see cref="OsuScreen"/> which spectates one or more users.
    /// </summary>
    public abstract partial class SpectatorScreen : OsuScreen
    {
        protected IReadOnlyList<int> Users => users;

        private readonly List<int> users = new List<int>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private readonly IBindableDictionary<int, SpectatorState> userStates = new BindableDictionary<int, SpectatorState>();

        private readonly Dictionary<int, APIUser> userMap = new Dictionary<int, APIUser>();
        private readonly Dictionary<int, SpectatorGameplayState> gameplayStates = new Dictionary<int, SpectatorGameplayState>();

        private IDisposable? realmSubscription;

        /// <summary>
        /// Creates a new <see cref="SpectatorScreen"/>.
        /// </summary>
        /// <param name="users">The users to spectate.</param>
        protected SpectatorScreen(params int[] users)
        {
            this.users.AddRange(users);
        }

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

                userStates.BindTo(spectatorClient.WatchedUserStates);
                userStates.BindCollectionChanged(onUserStatesChanged, true);

                realmSubscription = realm.RegisterForNotifications(
                    realm => realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending), beatmapsChanged);

                foreach ((int id, var _) in userMap)
                    spectatorClient.WatchUser(id);
            }));
        }

        private void beatmapsChanged(IRealmCollection<BeatmapSetInfo> items, ChangeSet? changes)
        {
            if (changes?.InsertedIndices == null) return;

            foreach (int c in changes.InsertedIndices) beatmapUpdated(items[c]);
        }

        private void beatmapUpdated(BeatmapSetInfo beatmapSet)
        {
            foreach ((int userId, _) in userMap)
            {
                if (!userStates.TryGetValue(userId, out var userState))
                    continue;

                if (beatmapSet.Beatmaps.Any(b => b.OnlineID == userState.BeatmapID))
                    startGameplay(userId);
            }
        }

        private void onUserStatesChanged(object? sender, NotifyDictionaryChangedEventArgs<int, SpectatorState> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                case NotifyDictionaryChangedAction.Replace:
                    foreach ((int userId, SpectatorState state) in e.NewItems.AsNonNull())
                        onUserStateChanged(userId, state);
                    break;
            }
        }

        private void onUserStateChanged(int userId, SpectatorState newState)
        {
            if (newState.RulesetID == null || newState.BeatmapID == null)
                return;

            if (!userMap.ContainsKey(userId))
                return;

            switch (newState.State)
            {
                case SpectatedUserState.Playing:
                    OnNewPlayingUserState(userId, newState);
                    startGameplay(userId);
                    break;

                case SpectatedUserState.Passed:
                    markReceivedAllFrames(userId);
                    PassGameplay(userId);
                    break;

                case SpectatedUserState.Failed:
                    failGameplay(userId);
                    break;

                case SpectatedUserState.Quit:
                    quitGameplay(userId);
                    break;
            }
        }

        private void startGameplay(int userId)
        {
            Debug.Assert(userMap.ContainsKey(userId));

            var user = userMap[userId];
            var spectatorState = userStates[userId];

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
            StartGameplay(userId, gameplayState);
        }

        /// <summary>
        /// Marks an existing gameplay session as received all frames.
        /// </summary>
        private void markReceivedAllFrames(int userId)
        {
            if (gameplayStates.TryGetValue(userId, out var gameplayState))
                gameplayState.Score.Replay.HasReceivedAllFrames = true;
        }

        private void failGameplay(int userId)
        {
            if (!userMap.ContainsKey(userId))
                return;

            if (!gameplayStates.ContainsKey(userId))
                return;

            markReceivedAllFrames(userId);

            gameplayStates.Remove(userId);
            FailGameplay(userId);
        }

        private void quitGameplay(int userId)
        {
            if (!userMap.ContainsKey(userId))
                return;

            if (!gameplayStates.ContainsKey(userId))
                return;

            markReceivedAllFrames(userId);

            gameplayStates.Remove(userId);
            QuitGameplay(userId);
        }

        /// <summary>
        /// Invoked when a spectated user's state has changed to a new state indicating the player is currently playing.
        /// Thread safety is not guaranteed – should be scheduled as required.
        /// </summary>
        /// <param name="userId">The user whose state has changed.</param>
        /// <param name="spectatorState">The new state.</param>
        protected abstract void OnNewPlayingUserState(int userId, SpectatorState spectatorState);

        /// <summary>
        /// Starts gameplay for a user.
        /// Thread safety is not guaranteed – should be scheduled as required.
        /// </summary>
        /// <param name="userId">The user to start gameplay for.</param>
        /// <param name="spectatorGameplayState">The gameplay state.</param>
        protected abstract void StartGameplay(int userId, SpectatorGameplayState spectatorGameplayState);

        /// <summary>
        /// Fired when a user passes gameplay.
        /// </summary>
        /// <param name="userId">The user which passed.</param>
        protected virtual void PassGameplay(int userId) { }

        /// <summary>
        /// Quits gameplay for a user.
        /// Thread safety is not guaranteed – should be scheduled as required.
        /// </summary>
        /// <param name="userId">The user to quit gameplay for.</param>
        protected abstract void QuitGameplay(int userId);

        /// <summary>
        /// Fails gameplay for a user.
        /// Thread safety is not guaranteed – should be scheduled as required.
        /// </summary>
        /// <param name="userId">The user to fail gameplay for.</param>
        protected abstract void FailGameplay(int userId);

        /// <summary>
        /// Stops spectating a user.
        /// </summary>
        /// <param name="userId">The user to stop spectating.</param>
        protected void RemoveUser(int userId)
        {
            if (!userStates.ContainsKey(userId))
                return;

            quitGameplay(userId);

            users.Remove(userId);
            userMap.Remove(userId);

            spectatorClient.StopWatchingUser(userId);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient.IsNotNull())
            {
                foreach ((int userId, var _) in userMap)
                    spectatorClient.StopWatchingUser(userId);
            }

            realmSubscription?.Dispose();
        }
    }
}
