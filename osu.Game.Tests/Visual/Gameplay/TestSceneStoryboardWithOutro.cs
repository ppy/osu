// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class TestSceneStoryboardWithOutro : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;

        protected new OutroPlayer Player => (OutroPlayer)base.Player;

        private double currentStoryboardDuration;

        private bool showResults = true;

        private event Func<HealthProcessor, JudgementResult, bool> currentFailConditions;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("enable storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, true));
            AddStep("set dim level to 0", () => LocalConfig.SetValue<double>(OsuSetting.DimLevel, 0));
            AddStep("reset fail conditions", () => currentFailConditions = (_, __) => false);
            AddStep("set storyboard duration to 2s", () => currentStoryboardDuration = 2000);
            AddStep("set ShowResults = true", () => showResults = true);
        }

        [Test]
        public void TestStoryboardSkipOutro()
        {
            CreateTest(null);
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddStep("skip outro", () => InputManager.Key(osuTK.Input.Key.Space));
            AddAssert("player is no longer current screen", () => !Player.IsCurrentScreen());
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardNoSkipOutro()
        {
            CreateTest(null);
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.GameplayClock.CurrentTime >= currentStoryboardDuration);
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardExitDuringOutroStillExits()
        {
            CreateTest(null);
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddStep("exit via pause", () => Player.ExitViaPause());
            AddAssert("player exited", () => !Player.IsCurrentScreen() && Player.GetChildScreen() == null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestStoryboardToggle(bool enabledAtBeginning)
        {
            CreateTest(null);
            AddStep($"{(enabledAtBeginning ? "enable" : "disable")} storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, enabledAtBeginning));
            AddStep("toggle storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, !enabledAtBeginning));
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestOutroEndsDuringFailAnimation()
        {
            CreateTest(() =>
            {
                AddStep("fail on first judgement", () => currentFailConditions = (_, __) => true);

                // Fail occurs at 164ms with the provided beatmap.
                // Fail animation runs for 2.5s realtime but the gameplay time change is *variable* due to the frequency transform being applied, so we need a bit of lenience.
                AddStep("set storyboard duration to 0.6s", () => currentStoryboardDuration = 600);
            });

            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.GameplayClock.CurrentTime >= currentStoryboardDuration);
            AddUntilStep("wait for fail overlay", () => Player.FailOverlay.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestShowResultsFalse()
        {
            CreateTest(() =>
            {
                AddStep("set ShowResults = false", () => showResults = false);
            });
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.GameplayClock.CurrentTime >= currentStoryboardDuration);
            AddWaitStep("wait", 10);
            AddAssert("no score shown", () => !Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardEndsBeforeCompletion()
        {
            CreateTest(() => AddStep("set storyboard duration to .1s", () => currentStoryboardDuration = 100));
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.GameplayClock.CurrentTime >= currentStoryboardDuration);
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardRewind()
        {
            SkipOverlay.FadeContainer fadeContainer() => Player.ChildrenOfType<SkipOverlay.FadeContainer>().First();

            CreateTest(null);
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddUntilStep("skip overlay content becomes visible", () => fadeContainer().State == Visibility.Visible);

            AddStep("rewind", () => Player.GameplayClockContainer.Seek(-1000));
            AddUntilStep("skip overlay content not visible", () => fadeContainer().State == Visibility.Hidden);

            AddUntilStep("skip overlay content becomes visible", () => fadeContainer().State == Visibility.Visible);
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.GameplayClock.CurrentTime >= currentStoryboardDuration);
        }

        [Test]
        public void TestPerformExitNoOutro()
        {
            CreateTest(null);
            AddStep("disable storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, false));
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddStep("exit via pause", () => Player.ExitViaPause());
            AddAssert("player exited", () => Stack.CurrentScreen == null);
        }

        protected override bool AllowFail => true;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new OutroPlayer(currentFailConditions, showResults);

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap();
            beatmap.HitObjects.Add(new HitCircle());
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

        protected class OutroPlayer : TestPlayer
        {
            public void ExitViaPause() => PerformExit(true);

            public new FailOverlay FailOverlay => base.FailOverlay;

            public bool IsScoreShown => !this.IsCurrentScreen() && this.GetChildScreen() is ResultsScreen;

            private event Func<HealthProcessor, JudgementResult, bool> failConditions;

            public OutroPlayer(Func<HealthProcessor, JudgementResult, bool> failConditions, bool showResults = true)
                : base(false, showResults)
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
