// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Storyboards;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneStoryboardWithOutro : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;

        protected override bool AllowBackwardsSeeks => true;

        protected new OutroPlayer Player => (OutroPlayer)base.Player;

        private double currentBeatmapDuration;
        private double currentStoryboardDuration;

        private bool showResults = true;

        private event Func<HealthProcessor, JudgementResult, bool> currentFailConditions;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("enable storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, true));
            AddStep("set dim level to 0", () => LocalConfig.SetValue<double>(OsuSetting.DimLevel, 0));
            AddStep("reset fail conditions", () => currentFailConditions = (_, _) => false);
            AddStep("set beatmap duration to 0s", () => currentBeatmapDuration = 0);
            AddStep("set storyboard duration to 8s", () => currentStoryboardDuration = 8000);
            AddStep("set ShowResults = true", () => showResults = true);
        }

        [Test]
        public void TestStoryboardSkipOutro()
        {
            AddStep("set storyboard duration to long", () => currentStoryboardDuration = 200000);
            CreateTest();
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddStep("skip outro", () => InputManager.Key(osuTK.Input.Key.Space));
            AddUntilStep("player is no longer current screen", () => !Player.IsCurrentScreen());
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardNoSkipOutro()
        {
            CreateTest();
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.CurrentTime >= currentStoryboardDuration);
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardExitDuringOutroProgressesToResults()
        {
            CreateTest();
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddStep("exit via pause", () => Player.ExitViaPause());
            AddUntilStep("reached results screen", () => Stack.CurrentScreen is ResultsScreen);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestStoryboardToggle(bool enabledAtBeginning)
        {
            CreateTest();
            AddStep($"{(enabledAtBeginning ? "enable" : "disable")} storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, enabledAtBeginning));
            AddStep("toggle storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, !enabledAtBeginning));
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestOutroEndsDuringFailAnimation()
        {
            CreateTest(() =>
            {
                AddStep("fail on first judgement", () => currentFailConditions = (_, _) => true);

                // Fail occurs at 164ms with the provided beatmap.
                // Fail animation runs for 2.5s realtime but the gameplay time change is *variable* due to the frequency transform being applied, so we need a bit of lenience.
                AddStep("set storyboard duration to 0.6s", () => currentStoryboardDuration = 600);
            });

            AddUntilStep("wait for fail", () => Player.GameplayState.HasFailed);
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.CurrentTime >= currentStoryboardDuration);
            AddUntilStep("wait for fail overlay", () => Player.FailOverlay.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestSaveFailedReplayWithStoryboardEndedDoesNotProgress()
        {
            CreateTest(() =>
            {
                AddStep("fail on first judgement", () => currentFailConditions = (_, _) => true);
                AddStep("set storyboard duration to 0s", () => currentStoryboardDuration = 0);
            });
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.CurrentTime >= currentStoryboardDuration);
            AddUntilStep("wait for fail", () => Player.GameplayState.HasFailed);

            AddUntilStep("wait for fail overlay", () => Player.FailOverlay.State.Value == Visibility.Visible);
            AddUntilStep("wait for button clickable", () => Player.ChildrenOfType<SaveFailedScoreButton>().First().ChildrenOfType<OsuClickableContainer>().First().Enabled.Value);
            AddStep("click save button", () => Player.ChildrenOfType<SaveFailedScoreButton>().First().ChildrenOfType<OsuClickableContainer>().First().TriggerClick());

            // Test a regression where importing the fail replay would cause progression to results screen in a failed state.
            AddWaitStep("wait some", 10);
            AddAssert("player is still current screen", () => Player.IsCurrentScreen());
        }

        [Test]
        public void TestShowResultsFalse()
        {
            CreateTest(() =>
            {
                AddStep("set ShowResults = false", () => showResults = false);
            });
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.CurrentTime >= currentStoryboardDuration);
            AddWaitStep("wait", 10);
            AddAssert("no score shown", () => !Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardEndsBeforeCompletion()
        {
            CreateTest(() => AddStep("set storyboard duration to .1s", () => currentStoryboardDuration = 100));
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.CurrentTime >= currentStoryboardDuration);
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardRewind()
        {
            SkipOverlay.FadeContainer fadeContainer() => Player.ChildrenOfType<SkipOverlay.FadeContainer>().First();

            CreateTest();
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddUntilStep("skip overlay content becomes visible", () => fadeContainer().State == Visibility.Visible);

            AddStep("rewind", () => Player.GameplayClockContainer.Seek(-1000));
            AddUntilStep("skip overlay content not visible", () => fadeContainer().State == Visibility.Hidden);

            AddUntilStep("skip overlay content becomes visible", () => fadeContainer().State == Visibility.Visible);
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.CurrentTime >= currentStoryboardDuration);
        }

        [Test]
        public void TestPerformExitNoOutro()
        {
            CreateTest();
            AddStep("disable storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, false));
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddStep("exit via pause", () => Player.ExitViaPause());
            AddUntilStep("reached results screen", () => Stack.CurrentScreen is ResultsScreen);
        }

        [Test]
        public void TestPerformExitAfterOutro()
        {
            CreateTest(() =>
            {
                AddStep("set beatmap duration to 4s", () => currentBeatmapDuration = 4000);
                AddStep("set storyboard duration to 1s", () => currentStoryboardDuration = 1000);
            });

            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.CurrentTime >= currentStoryboardDuration);
            AddStep("exit via pause", () => Player.ExitViaPause());
            AddAssert("player paused", () => !Player.IsResuming);

            AddStep("resume player", () => Player.Resume());
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        protected override bool AllowFail => true;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new OutroPlayer(currentFailConditions, showResults);

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap();
            beatmap.HitObjects.Add(new HitCircle { StartTime = currentBeatmapDuration });
            return beatmap;
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            return base.CreateWorkingBeatmap(beatmap, createStoryboard(currentStoryboardDuration));
        }

        private Storyboard createStoryboard(double duration)
        {
            var storyboard = new Storyboard();
            var sprite = new StoryboardSprite("unknown", Anchor.TopLeft, Vector2.Zero);
            sprite.TimelineGroup.Alpha.Add(Easing.None, 0, duration, 1, 0);
            storyboard.GetLayer("Background").Add(sprite);
            return storyboard;
        }

        protected partial class OutroPlayer : TestPlayer
        {
            public void ExitViaPause() => PerformExit(true);

            public new FailOverlay FailOverlay => base.FailOverlay;

            public bool IsScoreShown => !this.IsCurrentScreen() && this.GetChildScreen() is ResultsScreen;

            private event Func<HealthProcessor, JudgementResult, bool> failConditions;

            public OutroPlayer(Func<HealthProcessor, JudgementResult, bool> failConditions, bool showResults = true)
                : base(showResults: showResults)
            {
                this.failConditions = failConditions;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                HealthProcessor.FailConditions += failConditions;
            }

            protected override Task ImportScore(Score score)
            {
                return Task.CompletedTask;
            }
        }
    }
}
