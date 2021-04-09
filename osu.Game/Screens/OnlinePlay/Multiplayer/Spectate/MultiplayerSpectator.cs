// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Online.Spectator;
using osu.Game.Screens.Spectate;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectator : SpectatorScreen
    {
        private const double min_duration_to_allow_playback = 50;

        // Isolates beatmap/ruleset to this screen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public bool AllPlayersLoaded => instances.All(p => p?.PlayerLoaded == true);

        [Resolved]
        private SpectatorStreamingClient spectatorClient { get; set; }

        private readonly PlayerInstance[] instances;
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

            InternalChild = new GridContainer
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
            };

            // Todo: This is not quite correct - it should be per-user to adjust for other mod combinations.
            var playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);
            var scoreProcessor = Ruleset.Value.CreateInstance().CreateScoreProcessor();
            scoreProcessor.ApplyBeatmap(playableBeatmap);

            LoadComponentAsync(leaderboard = new MultiplayerSpectatorLeaderboard(scoreProcessor, UserIds) { Expanded = { Value = true } }, leaderboardContainer.Add);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            updateGameplayPlayingState();
        }

        private void updateGameplayPlayingState()
        {
            // Make sure all players are loaded and have frames before starting any.
            if (!AllPlayersLoaded || !instances.All(i => i.Score.Replay.Frames.Count > 0))
            {
                foreach (var inst in instances)
                    inst?.PauseGameplay();
                return;
            }

            double targetTrackTime = instances.Select(i => i.GetCurrentGameplayTime()).Max();

            foreach (var inst in instances)
            {
                Debug.Assert(inst != null);

                double lastFrameTime = inst.Score.Replay.Frames.Select(f => f.Time).Last();
                double currentTime = inst.GetCurrentGameplayTime();

                bool canContinuePlayback = Precision.DefinitelyBigger(lastFrameTime, currentTime, min_duration_to_allow_playback);
                if (!canContinuePlayback)
                    continue;

                inst.ContinueGameplay(targetTrackTime);
            }
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
                leaderboard.RemoveClock(existingInstance.User.Id);
            }

            LoadComponentAsync(instances[userIndex] = new PlayerInstance(gameplayState.Score), d =>
            {
                if (instances[userIndex] == d)
                {
                    grid.Add(d);
                    leaderboard.AddClock(d.User.Id, d.Beatmap.Track);
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
