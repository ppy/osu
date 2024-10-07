// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneStoryboardWithIntro : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;
        protected override bool AllowFail => true;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap();
            beatmap.HitObjects.Add(new HitCircle { StartTime = firstObjectStartTime });
            return beatmap;
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
        {
            return base.CreateWorkingBeatmap(beatmap, createStoryboard(storyboardStartTime));
        }

        private Storyboard createStoryboard(double startTime)
        {
            var storyboard = new Storyboard();
            var sprite = new StoryboardSprite("unknown", Anchor.TopLeft, Vector2.Zero);
            sprite.Commands.AddAlpha(Easing.None, startTime, 0, 0, 1);
            storyboard.GetLayer("Background").Add(sprite);
            return storyboard;
        }

        private double firstObjectStartTime;
        private double storyboardStartTime;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("enable storyboard", () => LocalConfig.SetValue(OsuSetting.ShowStoryboard, true));
            AddStep("set dim level to 0", () => LocalConfig.SetValue<double>(OsuSetting.DimLevel, 0));
            AddStep("reset first hitobject time", () => firstObjectStartTime = 0);
            AddStep("reset storyboard start time", () => storyboardStartTime = 0);
        }

        [TestCase(-5000, 0)]
        [TestCase(-5000, 30000)]
        public void TestStoryboardSingleSkip(double storyboardStart, double firstObject)
        {
            AddStep($"set storyboard start time to {storyboardStart}", () => storyboardStartTime = storyboardStart);
            AddStep($"set first object start time to {firstObject}", () => firstObjectStartTime = firstObject);
            CreateTest();

            AddStep("skip", () => InputManager.Key(osuTK.Input.Key.Space));
            AddAssert("skip performed", () => Player.ChildrenOfType<SkipOverlay>().Any(s => s.SkipCount == 1));
            AddUntilStep("gameplay clock advanced", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(firstObject - 2000));
        }

        [Test]
        public void TestStoryboardDoubleSkip()
        {
            AddStep("set storyboard start time to -11000", () => storyboardStartTime = -11000);
            AddStep("set first object start time to 11000", () => firstObjectStartTime = 11000);
            CreateTest();

            AddStep("skip", () => InputManager.Key(osuTK.Input.Key.Space));
            AddAssert("skip performed", () => Player.ChildrenOfType<SkipOverlay>().Any(s => s.SkipCount == 1));
            AddUntilStep("gameplay clock advanced", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(0));

            AddStep("skip", () => InputManager.Key(osuTK.Input.Key.Space));
            AddAssert("skip performed", () => Player.ChildrenOfType<SkipOverlay>().Any(s => s.SkipCount == 2));
            AddUntilStep("gameplay clock advanced", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(9000));
        }
    }
}
