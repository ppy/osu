// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning.Triangles;
using osu.Game.Tests.Gameplay;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestScenePerformancePointsCounter : SkinnableHUDComponentTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private readonly ScoreProcessor scoreProcessor = new OsuScoreProcessor();

        [Cached]
        private readonly GameplayState gameplayState = TestGameplayState.Create(new OsuRuleset());

        private int iteration;

        protected override Drawable CreateDefaultImplementation() => new TrianglesPerformancePointsCounter();
        protected override Drawable CreateArgonImplementation() => new ArgonPerformancePointsCounter();
        protected override Drawable CreateLegacyImplementation() => Empty();

        private Bindable<JudgementResult> lastJudgementResult => (Bindable<JudgementResult>)gameplayState.LastJudgementResult;

        public override void SetUpSteps()
        {
            AddStep("reset", () =>
            {
                var ruleset = new OsuRuleset();
                var beatmap = CreateWorkingBeatmap(ruleset.RulesetInfo)
                    .GetPlayableBeatmap(ruleset.RulesetInfo);

                iteration = 0;
                scoreProcessor.ApplyBeatmap(beatmap);
                lastJudgementResult.SetDefault();
            });

            base.SetUpSteps();
        }

        [Test]
        public void TestDisplay()
        {
            AddSliderStep("pp", 0, 2000, 0, v => this.ChildrenOfType<PerformancePointsCounter>().ForEach(c => c.Current.Value = v));
            AddToggleStep("toggle validity", v => this.ChildrenOfType<PerformancePointsCounter>().ForEach(c => c.IsValid = v));
        }

        [Test]
        public void TestBasicCounting()
        {
            int previousValue = 0;

            AddAssert("counter displaying zero", () => this.ChildrenOfType<PerformancePointsCounter>().All(c => c.Current.Value == 0));

            AddRepeatStep("Add judgement", applyOneJudgement, 10);

            AddUntilStep("counter non-zero", () => this.ChildrenOfType<PerformancePointsCounter>().All(c => c.Current.Value > 0));
            AddUntilStep("counter valid", () => this.ChildrenOfType<PerformancePointsCounter>().All(c => c.IsValid));

            AddStep("Revert judgement", () =>
            {
                previousValue = this.ChildrenOfType<PerformancePointsCounter>().First().Current.Value;

                scoreProcessor.RevertResult(new JudgementResult(new HitObject(), new OsuJudgement()));
            });

            AddUntilStep("counter decreased", () => this.ChildrenOfType<PerformancePointsCounter>().All(c => c.Current.Value < previousValue));

            AddStep("Add judgement", applyOneJudgement);

            AddUntilStep("counter non-zero", () => this.ChildrenOfType<PerformancePointsCounter>().All(c => c.Current.Value > 0));
        }

        [Test]
        public void TestCounterUpdatesWithJudgementsBeforeCreation()
        {
            AddRepeatStep("Add judgement", applyOneJudgement, 10);

            AddStep("recreate counter", SetUpComponents);

            AddUntilStep("counter non-zero", () => this.ChildrenOfType<PerformancePointsCounter>().All(c => c.Current.Value > 0));
            AddUntilStep("counter valid", () => this.ChildrenOfType<PerformancePointsCounter>().All(c => c.IsValid));
        }

        private void applyOneJudgement()
        {
            var scoreInfo = gameplayState.Score.ScoreInfo;

            scoreInfo.MaxCombo = iteration * 1000;
            scoreInfo.Accuracy = 1;
            scoreInfo.Statistics[HitResult.Great] = iteration * 1000;

            lastJudgementResult.Value = new OsuJudgementResult(new HitObject
            {
                StartTime = iteration * 10000,
            }, new OsuJudgement())
            {
                Type = HitResult.Perfect,
            };
            scoreProcessor.ApplyResult(lastJudgementResult.Value);

            iteration++;
        }
    }
}
