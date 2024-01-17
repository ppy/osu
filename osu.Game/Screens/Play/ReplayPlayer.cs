// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Screens.Ranking;
using osu.Game.Users;

namespace osu.Game.Screens.Play
{
    public partial class ReplayPlayer : Player, IKeyBindingHandler<GlobalAction>
    {
        private readonly Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore;

        private readonly bool replayIsFailedScore;

        protected override UserActivity InitialActivity => new UserActivity.WatchingReplay(Score.ScoreInfo);

        // Disallow replays from failing. (see https://github.com/ppy/osu/issues/6108)
        protected override bool CheckModsAllowFailure()
        {
            if (!replayIsFailedScore && !GameplayState.Mods.OfType<ModAutoplay>().Any())
                return false;

            return base.CheckModsAllowFailure();
        }

        public ReplayPlayer(Score score, PlayerConfiguration configuration = null)
            : this((_, _) => score, configuration)
        {
            replayIsFailedScore = score.ScoreInfo.Rank == ScoreRank.F;
        }

        public ReplayPlayer(Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore, PlayerConfiguration configuration = null)
            : base(configuration)
        {
            this.createScore = createScore;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!LoadedBeatmapSuccessfully)
                return;

            var playbackSettings = new PlaybackSettings
            {
                Depth = float.MaxValue,
                Expanded = { Value = false }
            };

            if (GameplayClockContainer is MasterGameplayClockContainer master)
                playbackSettings.UserPlaybackRate.BindTo(master.UserPlaybackRate);

            HUDOverlay.PlayerSettingsOverlay.AddAtStart(playbackSettings);
        }

        protected override void PrepareReplay()
        {
            DrawableRuleset?.SetReplayScore(Score);
        }

        protected override Score CreateScore(IBeatmap beatmap) => createScore(beatmap, Mods.Value);

        // Don't re-import replay scores as they're already present in the database.
        protected override Task ImportScore(Score score) => Task.CompletedTask;

        public readonly BindableList<ScoreInfo> LeaderboardScores = new BindableList<ScoreInfo>();

        protected override GameplayLeaderboard CreateGameplayLeaderboard() =>
            new SoloGameplayLeaderboard(Score.ScoreInfo.User)
            {
                AlwaysVisible = { Value = true },
                Scores = { BindTarget = LeaderboardScores }
            };

        protected override ResultsScreen CreateResults(ScoreInfo score) => new SoloResultsScreen(score, false);

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            const double keyboard_seek_amount = 5000;

            switch (e.Action)
            {
                case GlobalAction.SeekReplayBackward:
                    keyboardSeek(-1);
                    return true;

                case GlobalAction.SeekReplayForward:
                    keyboardSeek(1);
                    return true;

                case GlobalAction.TogglePauseReplay:
                    if (GameplayClockContainer.IsPaused.Value)
                        GameplayClockContainer.Start();
                    else
                        GameplayClockContainer.Stop();
                    return true;
            }

            return false;

            void keyboardSeek(int direction)
            {
                double target = Math.Clamp(GameplayClockContainer.CurrentTime + direction * keyboard_seek_amount, 0, GameplayState.Beatmap.GetLastObjectTime());

                Seek(target);
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
