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
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD.JudgementCounter;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneJudgementCounter : OsuTestScene
    {
        private ScoreProcessor scoreProcessor = null!;
        private JudgementCountController judgementCountController = null!;
        private TestJudgementCounterDisplay counterDisplay = null!;

        private DependencyProvidingContainer content = null!;

        protected override Container<Drawable> Content => content;

        private readonly Bindable<Judgement> lastJudgementResult = new Bindable<Judgement>();

        private int iteration;

        [SetUpSteps]
        public void SetUpSteps() => AddStep("Create components", () =>
        {
            var ruleset = CreateRuleset();

            Debug.Assert(ruleset != null);

            scoreProcessor = new ScoreProcessor(ruleset);
            base.Content.Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[] { (typeof(ScoreProcessor), scoreProcessor), (typeof(Ruleset), ruleset) },
                Children = new Drawable[]
                {
                    judgementCountController = new JudgementCountController(),
                    content = new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[] { (typeof(JudgementCountController), judgementCountController) },
                    }
                },
            };
        });

        protected override Ruleset CreateRuleset() => new OsuRuleset();

        private void applyOneJudgement(HitResult result)
        {
            lastJudgementResult.Value = new OsuJudgement(new HitObject
            {
                StartTime = iteration * 10000
            }, new OsuJudgementInfo())
            {
                Type = result,
            };
            scoreProcessor.ApplyResult(lastJudgementResult.Value);

            iteration++;
        }

        [Test]
        public void TestAddJudgementsToCounters()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestJudgementCounterDisplay());

            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.Great), 2);
            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.Miss), 2);
            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.Meh), 2);
        }

        [Test]
        public void TestAddWhilstHidden()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestJudgementCounterDisplay());

            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.LargeTickHit), 2);
            AddAssert("Check value added whilst hidden", () => hiddenCount() == 2);
            AddStep("Show all judgements", () => counterDisplay.Mode.Value = JudgementCounterDisplay.DisplayMode.All);
        }

        [Test]
        public void TestChangeFlowDirection()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestJudgementCounterDisplay());

            AddStep("Set direction vertical", () => counterDisplay.FlowDirection.Value = Direction.Vertical);
            AddStep("Set direction horizontal", () => counterDisplay.FlowDirection.Value = Direction.Horizontal);
        }

        [Test]
        public void TestToggleJudgementNames()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestJudgementCounterDisplay());

            AddStep("Hide judgement names", () => counterDisplay.ShowJudgementNames.Value = false);
            AddWaitStep("wait some", 2);
            AddAssert("Assert hidden", () => counterDisplay.CounterFlow.Children.First().ResultName.Alpha == 0);
            AddStep("Hide judgement names", () => counterDisplay.ShowJudgementNames.Value = true);
            AddWaitStep("wait some", 2);
            AddAssert("Assert shown", () => counterDisplay.CounterFlow.Children.First().ResultName.Alpha == 1);
        }

        [Test]
        public void TestHideMaxValue()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestJudgementCounterDisplay());

            AddStep("Hide max judgement", () => counterDisplay.ShowMaxJudgement.Value = false);
            AddWaitStep("wait some", 2);
            AddAssert("Check max hidden", () => counterDisplay.CounterFlow.ChildrenOfType<JudgementCounter>().First().Alpha == 0);
            AddStep("Show max judgement", () => counterDisplay.ShowMaxJudgement.Value = true);
        }

        [Test]
        public void TestMaxValueStartsHidden()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestJudgementCounterDisplay
            {
                ShowMaxJudgement = { Value = false }
            });
            AddAssert("Check max hidden", () => counterDisplay.CounterFlow.ChildrenOfType<JudgementCounter>().First().Alpha == 0);
        }

        [Test]
        public void TestMaxValueHiddenOnModeChange()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestJudgementCounterDisplay());

            AddStep("Set max judgement to hide itself", () => counterDisplay.ShowMaxJudgement.Value = false);
            AddStep("Show all judgements", () => counterDisplay.Mode.Value = JudgementCounterDisplay.DisplayMode.All);
            AddWaitStep("wait some", 2);
            AddAssert("Assert max judgement hidden", () => counterDisplay.CounterFlow.ChildrenOfType<JudgementCounter>().First().Alpha == 0);
        }

        [Test]
        public void TestNoDuplicates()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestJudgementCounterDisplay());
            AddStep("Show all judgements", () => counterDisplay.Mode.Value = JudgementCounterDisplay.DisplayMode.All);
            AddAssert("Check no duplicates",
                () => counterDisplay.CounterFlow.ChildrenOfType<JudgementCounter>().Count(),
                () => Is.EqualTo(counterDisplay.CounterFlow.ChildrenOfType<JudgementCounter>().Select(c => c.ResultName.Text).Distinct().Count()));
        }

        [Test]
        public void TestCycleDisplayModes()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestJudgementCounterDisplay());

            AddStep("Show basic judgements", () => counterDisplay.Mode.Value = JudgementCounterDisplay.DisplayMode.Simple);
            AddWaitStep("wait some", 2);
            AddAssert("Check only basic", () => counterDisplay.CounterFlow.ChildrenOfType<JudgementCounter>().Last().Alpha == 0);
            AddStep("Show normal judgements", () => counterDisplay.Mode.Value = JudgementCounterDisplay.DisplayMode.Normal);
            AddStep("Show all judgements", () => counterDisplay.Mode.Value = JudgementCounterDisplay.DisplayMode.All);
            AddWaitStep("wait some", 2);
            AddAssert("Check all visible", () => counterDisplay.CounterFlow.ChildrenOfType<JudgementCounter>().Last().Alpha == 1);
        }

        private int hiddenCount()
        {
            var num = counterDisplay.CounterFlow.Children.First(child => child.Result.Types.Contains(HitResult.LargeTickHit));
            return num.Result.ResultCount.Value;
        }

        private partial class TestJudgementCounterDisplay : JudgementCounterDisplay
        {
            public new FillFlowContainer<JudgementCounter> CounterFlow => base.CounterFlow;

            public TestJudgementCounterDisplay()
            {
                Margin = new MarginPadding { Top = 100 };
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
            }
        }
    }
}
