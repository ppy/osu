// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayer : Player, IKeyBindingHandler<GlobalAction>
    {
        protected Score Score { get; private set; }

        private readonly Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore;

        // Disallow replays from failing. (see https://github.com/ppy/osu/issues/6108)
        protected override bool CheckModsAllowFailure() => false;

        public ReplayPlayer(Score score, PlayerConfiguration configuration = null)
            : this((_, __) => score, configuration)
        {
        }

        public ReplayPlayer(Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore, PlayerConfiguration configuration = null)
            : base(configuration)
        {
            this.createScore = createScore;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!LoadedBeatmapSuccessfully) return;

            Score = createScore(GameplayBeatmap.PlayableBeatmap, Mods.Value);
        }

        protected override void PrepareReplay()
        {
            DrawableRuleset?.SetReplayScore(Score);
        }

        protected override Score CreateScore()
        {
            var baseScore = base.CreateScore();

            // Since the replay score doesn't contain statistics, we'll pass them through here.
            Score.ScoreInfo.HitEvents = baseScore.ScoreInfo.HitEvents;

            return Score;
        }

        // Don't re-import replay scores as they're already present in the database.
        protected override Task ImportScore(Score score) => Task.CompletedTask;

        protected override ResultsScreen CreateResults(ScoreInfo score) => new SoloResultsScreen(score, false);

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.TogglePauseReplay:
                    if (GameplayClockContainer.IsPaused.Value)
                        GameplayClockContainer.Start();
                    else
                        GameplayClockContainer.Stop();
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }
    }
}
