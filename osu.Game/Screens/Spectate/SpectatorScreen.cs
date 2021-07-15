// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Spectator;
using osu.Game.Replays;
using osu.Game.Rulesets;
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
        private SpectatorClient spectatorClient { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        private readonly IBindableDictionary<int, SpectatorState> playingUserStates = new BindableDictionary<int, SpectatorState>();

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

            userLookupCache.GetUsersAsync(userIds.ToArray()).ContinueWith(users => Schedule(() =>
            {
                foreach (var u in users.Result)
                {
                    if (u == null)
                        continue;

                    userMap[u.Id] = u;
                }

                playingUserStates.BindTo(spectatorClient.PlayingUserStates);
                playingUserStates.BindCollectionChanged(onPlayingUserStatesChanged, true);

                managerUpdated = beatmaps.ItemUpdated.GetBoundCopy();
                managerUpdated.BindValueChanged(beatmapUpdated);

                foreach (var (id, _) in userMap)
                    spectatorClient.WatchUser(id);
            }));
        }

        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> e)
        {
            if (!e.NewValue.TryGetTarget(out var beatmapSet))
                return;

            foreach (var (userId, _) in userMap)
            {
                if (!playingUserStates.TryGetValue(userId, out var userState))
                    continue;

                if (beatmapSet.Beatmaps.Any(b => b.OnlineBeatmapID == userState.BeatmapID))
                    updateGameplayState(userId);
            }
        }

        private void onPlayingUserStatesChanged(object sender, NotifyDictionaryChangedEventArgs<int, SpectatorState> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    foreach (var (userId, state) in e.NewItems.AsNonNull())
                        onUserStateAdded(userId, state);
                    break;

                case NotifyDictionaryChangedAction.Remove:
                    foreach (var (userId, _) in e.OldItems.AsNonNull())
                        onUserStateRemoved(userId);
                    break;

                case NotifyDictionaryChangedAction.Replace:
                    foreach (var (userId, _) in e.OldItems.AsNonNull())
                        onUserStateRemoved(userId);

                    foreach (var (userId, state) in e.NewItems.AsNonNull())
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
            onUserStateRemoved(userId);

            userIds.Remove(userId);
            userMap.Remove(userId);

            spectatorClient.StopWatchingUser(userId);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient != null)
            {
                foreach (var (userId, _) in userMap)
                    spectatorClient.StopWatchingUser(userId);
            }

            managerUpdated?.UnbindAll();
        }
    }
}
