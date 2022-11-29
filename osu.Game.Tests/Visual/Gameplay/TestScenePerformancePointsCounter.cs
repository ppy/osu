// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestScenePerformancePointsCounter : OsuTestScene
    {
        private DependencyProvidingContainer dependencyContainer;

        private GameplayState gameplayState;
        private ScoreProcessor scoreProcessor;

        private int iteration;
        private Bindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();
        private PerformancePointsCounter counter;

        [SetUpSteps]
        public void SetUpSteps() => AddStep("create components", () =>
        {
            var ruleset = CreateRuleset();

            Debug.Assert(ruleset != null);

            var beatmap = CreateWorkingBeatmap(ruleset.RulesetInfo)
                .GetPlayableBeatmap(ruleset.RulesetInfo);

            lastJudgementResult = new Bindable<JudgementResult>();

            gameplayState = new GameplayState(beatmap, ruleset);
            gameplayState.LastJudgementResult.BindTo(lastJudgementResult);

            scoreProcessor = new ScoreProcessor(ruleset);

            Child = dependencyContainer = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(GameplayState), gameplayState),
                    (typeof(ScoreProcessor), scoreProcessor)
                }
            };

            iteration = 0;
        });

        protected override Ruleset CreateRuleset() => new OsuRuleset();

        private void createCounter() => AddStep("Create counter", () =>
        {
            dependencyContainer.Child = counter = new PerformancePointsCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(5),
            };
        });

        [Test]
        public void TestBasicCounting()
        {
            int previousValue = 0;
            createCounter();

            AddAssert("counter displaying zero", () => counter.Current.Value == 0);

            AddRepeatStep("Add judgement", applyOneJudgement, 10);

            AddUntilStep("counter non-zero", () => counter.Current.Value > 0);
            AddUntilStep("counter opaque", () => counter.Child.Alpha == 1);

            AddStep("Revert judgement", () =>
            {
                previousValue = counter.Current.Value;

                scoreProcessor.RevertResult(new JudgementResult(new HitObject(), new OsuJudgement()));
            });

            AddUntilStep("counter decreased", () => counter.Current.Value < previousValue);

            AddStep("Add judgement", applyOneJudgement);

            AddUntilStep("counter non-zero", () => counter.Current.Value > 0);
        }

        [Test]
        public void TestCounterUpdatesWithJudgementsBeforeCreation()
        {
            AddRepeatStep("Add judgement", applyOneJudgement, 10);

            createCounter();

            AddUntilStep("counter non-zero", () => counter.Current.Value > 0);
            AddUntilStep("counter opaque", () => counter.Child.Alpha == 1);
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
