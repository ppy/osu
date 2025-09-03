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
using osu.Game.Skinning.Components;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneArgonJudgementCounter : OsuTestScene
    {
        private ScoreProcessor scoreProcessor = null!;
        private JudgementCountController judgementCountController = null!;
        private TestArgonJudgementCounterDisplay counterDisplay = null!;

        private DependencyProvidingContainer content = null!;

        protected override Container<Drawable> Content => content;

        private readonly Bindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();

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
            AddStep("create counter", () => Child = counterDisplay = new TestArgonJudgementCounterDisplay());

            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.Great), 2);
            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.Miss), 2);
            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.Meh), 2);
        }

        [Test]
        public void TestAddWhilstHidden()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestArgonJudgementCounterDisplay());

            AddRepeatStep("Add judgement", () => applyOneJudgement(HitResult.LargeTickHit), 2);
            AddAssert("Check value added whilst hidden", () => hiddenCount() == 2);
            AddStep("Show all judgements", () => counterDisplay.Mode.Value = ArgonJudgementCounterDisplay.DisplayMode.All);
        }

        [Test]
        public void TestChangeFlowDirection()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestArgonJudgementCounterDisplay());

            AddStep("Set direction vertical", () => counterDisplay.FlowDirection.Value = Direction.Vertical);
            AddStep("Set direction horizontal", () => counterDisplay.FlowDirection.Value = Direction.Horizontal);

            AddStep("add 100 ok judgements", () =>
            {
                for (int i = 0; i < 100; i++)
                    applyOneJudgement(HitResult.Ok);
            });
            AddStep("add 1000 great judgements", () =>
            {
                for (int i = 0; i < 1000; i++)
                    applyOneJudgement(HitResult.Great);
            });

            AddToggleStep("toggle max judgement display", t => counterDisplay.ShowMaxJudgement.Value = t);
        }

        [Test]
        public void TestToggleJudgementNames()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestArgonJudgementCounterDisplay());

            AddStep("Show label", () => counterDisplay.ShowLabel.Value = true);
            AddWaitStep("wait some", 2);
            AddAssert("Assert hidden", () => counterDisplay.CounterFlow.Children.First().Alpha == 1);
            AddStep("Hide label", () => counterDisplay.ShowLabel.Value = false);
            AddWaitStep("wait some", 2);
            AddAssert("Assert shown", () => counterDisplay.CounterFlow.Children.First().Alpha == 1);
        }

        [Test]
        public void TestHideMaxValue()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestArgonJudgementCounterDisplay());

            AddStep("Hide max judgement", () => counterDisplay.ShowMaxJudgement.Value = false);
            AddWaitStep("wait some", 2);
            AddAssert("Check max hidden", () => counterDisplay.CounterFlow.ChildrenOfType<ArgonJudgementCounter>().First().Alpha == 0);
            AddStep("Show max judgement", () => counterDisplay.ShowMaxJudgement.Value = true);
        }

        [Test]
        public void TestMaxValueStartsHidden()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestArgonJudgementCounterDisplay
            {
                ShowMaxJudgement = { Value = false }
            });
            AddAssert("Check max hidden", () => counterDisplay.CounterFlow.ChildrenOfType<ArgonJudgementCounter>().First().Alpha == 0);
        }

        [Test]
        public void TestMaxValueHiddenOnModeChange()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestArgonJudgementCounterDisplay());

            AddStep("Set max judgement to hide itself", () => counterDisplay.ShowMaxJudgement.Value = false);
            AddStep("Show all judgements", () => counterDisplay.Mode.Value = ArgonJudgementCounterDisplay.DisplayMode.All);
            AddWaitStep("wait some", 2);
            AddAssert("Assert max judgement hidden", () => counterDisplay.CounterFlow.ChildrenOfType<ArgonJudgementCounter>().First().Alpha == 0);
        }

        [Test]
        public void TestNoDuplicates()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestArgonJudgementCounterDisplay());
            AddStep("Show all judgements", () => counterDisplay.Mode.Value = ArgonJudgementCounterDisplay.DisplayMode.All);
            AddAssert("Check no duplicates",
                () => counterDisplay.CounterFlow.ChildrenOfType<ArgonJudgementCounter>().Count(),
                () => Is.EqualTo(counterDisplay.CounterFlow.ChildrenOfType<ArgonJudgementCounter>().Select(c => c.Result.DisplayName).Distinct().Count()));
        }

        [Test]
        public void TestCycleDisplayModes()
        {
            AddStep("create counter", () => Child = counterDisplay = new TestArgonJudgementCounterDisplay());

            AddStep("Show basic judgements", () => counterDisplay.Mode.Value = ArgonJudgementCounterDisplay.DisplayMode.Simple);
            AddWaitStep("wait some", 2);
            AddAssert("Check only basic", () => counterDisplay.CounterFlow.ChildrenOfType<ArgonJudgementCounter>().Last().Alpha == 0);
            AddStep("Show normal judgements", () => counterDisplay.Mode.Value = ArgonJudgementCounterDisplay.DisplayMode.Normal);
            AddStep("Show all judgements", () => counterDisplay.Mode.Value = ArgonJudgementCounterDisplay.DisplayMode.All);
            AddWaitStep("wait some", 2);
            AddAssert("Check all visible", () => counterDisplay.CounterFlow.ChildrenOfType<ArgonJudgementCounter>().Last().Alpha == 1);
        }

        private int hiddenCount()
        {
            var num = counterDisplay.CounterFlow.Children.First(child => child.Result.Types.Contains(HitResult.LargeTickHit));
            return num.Result.ResultCount.Value;
        }

        private partial class TestArgonJudgementCounterDisplay : ArgonJudgementCounterDisplay
        {
            public new FillFlowContainer<ArgonJudgementCounter> CounterFlow => base.CounterFlow;

            public TestArgonJudgementCounterDisplay()
            {
                Margin = new MarginPadding { Top = 100 };
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
            }
        }
    }
}
