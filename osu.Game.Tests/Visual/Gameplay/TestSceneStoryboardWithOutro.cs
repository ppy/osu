// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Storyboards;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneStoryboardWithOutro : PlayerTestScene
    {
        protected new OutroPlayer Player => (OutroPlayer)base.Player;

        private const double storyboard_duration = 2000;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("enable storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, true));
            AddStep("set dim level to 0", () => LocalConfig.SetValue<double>(OsuSetting.DimLevel, 0));
        }

        [Test]
        public void TestStoryboardSkipOutro()
        {
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddStep("skip outro", () => InputManager.Key(osuTK.Input.Key.Space));
            AddAssert("score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardNoSkipOutro()
        {
            AddUntilStep("storyboard ends", () => Player.GameplayClockContainer.GameplayClock.CurrentTime >= storyboard_duration);
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        [Test]
        public void TestStoryboardExitToSkipOutro()
        {
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
            AddStep("exit via pause", () => Player.ExitViaPause());
            AddAssert("score shown", () => Player.IsScoreShown);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestStoryboardToggle(bool enabledAtBeginning)
        {
            AddStep($"{(enabledAtBeginning ? "enable" : "disable")} storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, enabledAtBeginning));
            AddStep("toggle storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, !enabledAtBeginning));
            AddUntilStep("wait for score shown", () => Player.IsScoreShown);
        }

        protected override bool AllowFail => false;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new OutroPlayer();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap();
            beatmap.HitObjects.Add(new HitCircle());
            return beatmap;
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            if (storyboard == null)
            {
                storyboard = new Storyboard();
                var sprite = new StoryboardSprite("unknown", Anchor.TopLeft, Vector2.Zero);
                sprite.TimelineGroup.Alpha.Add(Easing.None, 0, storyboard_duration, 1, 0);
                storyboard.GetLayer("Background").Add(sprite);
            }

            return base.CreateWorkingBeatmap(beatmap, storyboard);
        }

        protected class OutroPlayer : TestPlayer
        {
            public void ExitViaPause() => PerformExit(true);

            public bool IsScoreShown => !this.IsCurrentScreen() && this.GetChildScreen() is ResultsScreen;

            public OutroPlayer()
                : base(false)
            {
            }

            protected override Task ImportScore(Score score)
            {
                return Task.CompletedTask;
            }
        }
    }
}
