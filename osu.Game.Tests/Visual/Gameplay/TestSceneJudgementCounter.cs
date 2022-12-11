// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD.JudgementCounter;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneJudgementCounter : OsuTestScene
    {
        private ScoreProcessor scoreProcessor = null!;
        private JudgementTally judgementTally = null!;
        private TestJudgementCounterDisplay counter = null!;

        private readonly Bindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();

        private int iteration;

        [SetUpSteps]
        public void SetupSteps() => AddStep("Create components", () =>
        {
            var ruleset = CreateRuleset();

            Debug.Assert(ruleset != null);

            scoreProcessor = new ScoreProcessor(ruleset);
            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[] { (typeof(ScoreProcessor), scoreProcessor), (typeof(Ruleset), ruleset) },
                Children = new Drawable[]
                {
                    judgementTally = new JudgementTally(),
                    new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[] { (typeof(JudgementTally), judgementTally) },
                        Child = counter = new TestJudgementCounterDisplay
                        {
                            Margin = new MarginPadding { Top = 100 },
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        }
                    }
                },
            };
        });

        protected override Ruleset CreateRuleset() => new ManiaRuleset();

        private void applyOneJudgement(HitResult result)
        {
            lastJudgementResult.Value = new OsuJudgementResult(new HitObject
            {
                StartTime = iteration * 10000
            }, new OsuJudgement())
            {
                Type = result,
            };
            scoreProcessor.ApplyResult(lastJudgementResult.Value);

            iteration++;
        }

        [Test]
        public void TestAddJudgementsToCounters()
        {
            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.Great), 2);
            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.Miss), 2);
            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.Meh), 2);
            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.LargeTickHit), 2);
            AddStep("Show all judgements", () => counter.Mode.Value = JudgementCounterDisplay.DisplayMode.All);
            AddAssert("Check value added whilst hidden", () => hiddenCount() == 2);
        }

        [Test]
        public void TestAddWhilstHidden()
        {
            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.LargeTickHit), 2);
            AddAssert("Check value added whilst hidden", () => hiddenCount() == 2);
            AddStep("Show all judgements", () => counter.Mode.Value = JudgementCounterDisplay.DisplayMode.All);
        }

        [Test]
        public void TestChangeFlowDirection()
        {
            AddStep("Set direction vertical", () => counter.FlowDirection.Value = JudgementCounterDisplay.Flow.Vertical);
            AddStep("Set direction horizontal", () => counter.FlowDirection.Value = JudgementCounterDisplay.Flow.Horizonal);
        }

        [Test]
        public void TestToggleJudgementNames()
        {
            AddStep("Hide judgement names", () => counter.ShowName.Value = false);
            AddAssert("Assert hidden", () => counter.JudgementContainer.Children.OfType<JudgementCounter>().First().ResultName.Alpha == 0);
            AddStep("Hide judgement names", () => counter.ShowName.Value = true);
            AddAssert("Assert shown", () => counter.JudgementContainer.Children.OfType<JudgementCounter>().First().ResultName.Alpha == 1);
        }

        [Test]
        public void TestHideMaxValue()
        {
            AddStep("Hide max judgement", () => counter.ShowMax.Value = false);
            AddStep("Show max judgement", () => counter.ShowMax.Value = true);
        }

        [Test]
        public void TestCycleDisplayModes()
        {
            AddStep("Show all judgements", () => counter.Mode.Value = JudgementCounterDisplay.DisplayMode.All);
            AddStep("Show normal judgements", () => counter.Mode.Value = JudgementCounterDisplay.DisplayMode.Normal);
            AddStep("Show basic judgements", () => counter.Mode.Value = JudgementCounterDisplay.DisplayMode.Simple);
        }

        private int hiddenCount()
        {
            var num = counter.JudgementContainer.Children.OfType<JudgementCounter>().First(child => child.Result.ResultInfo.Type == HitResult.LargeTickHit);
            return num.Result.ResultCount.Value;
        }

        private partial class TestJudgementCounterDisplay : JudgementCounterDisplay
        {
            public new FillFlowContainer JudgementContainer => base.JudgementContainer;
        }
    }
}
