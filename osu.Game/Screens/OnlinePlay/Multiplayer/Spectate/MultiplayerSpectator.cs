// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Spectator;
using osu.Game.Screens.Play;
using osu.Game.Screens.Spectate;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectator : SpectatorScreen
    {
        // Isolates beatmap/ruleset to this screen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public bool AllPlayersLoaded => instances.All(p => p?.PlayerLoaded == true);

        [Resolved]
        private SpectatorStreamingClient spectatorClient { get; set; }

        private readonly PlayerInstance[] instances;
        private MasterGameplayClockContainer masterClockContainer;
        private IMultiplayerSpectatorSyncManager syncManager;
        private PlayerGrid grid;
        private MultiplayerSpectatorLeaderboard leaderboard;

        public MultiplayerSpectator(int[] userIds)
            : base(userIds.AsSpan().Slice(0, Math.Min(16, userIds.Length)).ToArray())
        {
            instances = new PlayerInstance[userIds.Length];
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Container leaderboardContainer;

            masterClockContainer = new MasterGameplayClockContainer(Beatmap.Value, 0)
            {
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            leaderboardContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X
                            },
                            grid = new PlayerGrid { RelativeSizeAxes = Axes.Both }
                        }
                    }
                }
            };

            InternalChildren = new[]
            {
                (Drawable)(syncManager = new MultiplayerCatchupSyncManager(masterClockContainer)),
                masterClockContainer
            };

            // Todo: This is not quite correct - it should be per-user to adjust for other mod combinations.
            var playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);
            var scoreProcessor = Ruleset.Value.CreateInstance().CreateScoreProcessor();
            scoreProcessor.ApplyBeatmap(playableBeatmap);

            LoadComponentAsync(leaderboard = new MultiplayerSpectatorLeaderboard(scoreProcessor, UserIds) { Expanded = { Value = true } }, leaderboardContainer.Add);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            masterClockContainer.Stop();
            masterClockContainer.Restart();
        }

        protected override void OnUserStateChanged(int userId, SpectatorState spectatorState)
        {
        }

        protected override void StartGameplay(int userId, GameplayState gameplayState)
        {
            int userIndex = getIndexForUser(userId);
            var existingInstance = instances[userIndex];

            if (existingInstance != null)
            {
                grid.Remove(existingInstance);
                syncManager.RemoveSlave(existingInstance.GameplayClock);
                leaderboard.RemoveClock(existingInstance.User.Id);
            }

            LoadComponentAsync(instances[userIndex] = new PlayerInstance(gameplayState.Score, new MultiplayerSpectatorSlaveClock(masterClockContainer.GameplayClock)), d =>
            {
                if (instances[userIndex] == d)
                {
                    grid.Add(d);
                    syncManager.AddSlave(d.GameplayClock);
                    leaderboard.AddClock(d.User.Id, d.GameplayClock);
                }
            });
        }

        protected override void EndGameplay(int userId)
        {
            spectatorClient.StopWatchingUser(userId);
            leaderboard.RemoveClock(userId);
        }

        private int getIndexForUser(int userId) => Array.IndexOf(UserIds, userId);
    }
}
