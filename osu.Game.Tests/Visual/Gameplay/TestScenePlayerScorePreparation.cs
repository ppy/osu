// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePlayerScorePreparation : OsuPlayerTestScene
    {
        protected override bool AllowFail => false;

        protected new PreparingPlayer Player => (PreparingPlayer)base.Player;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            // Ensure track has actually running before attempting to seek
            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
        }

        [Test]
        public void TestPreparationOnResults()
        {
            AddUntilStep("wait for preparation", () => Player.PreparationCompleted);
        }

        [Test]
        public void TestPreparationOnExit()
        {
            AddStep("exit", () => Player.Exit());
            AddUntilStep("wait for preparation", () => Player.PreparationCompleted);
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new PreparingPlayer();

        public class PreparingPlayer : TestPlayer
        {
            public bool PreparationCompleted { get; private set; }

            public bool ResultsCreated { get; private set; }

            public PreparingPlayer()
                : base(true, true)
            {
            }

            protected override ResultsScreen CreateResults(ScoreInfo score)
            {
                var results = base.CreateResults(score);
                ResultsCreated = true;
                return results;
            }

            protected override Task PrepareScoreForResultsAsync(Score score)
            {
                PreparationCompleted = true;
                return base.PrepareScoreForResultsAsync(score);
            }
        }
    }
}
