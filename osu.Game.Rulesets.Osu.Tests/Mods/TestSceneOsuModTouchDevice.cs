// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModTouchDevice : PlayerTestScene
    {
        private TestOnScreenDisplay testOnScreenDisplay = null!;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) =>
            new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle
                    {
                        Position = OsuPlayfield.BASE_SIZE / 2,
                        StartTime = 0,
                    },
                    new HitCircle
                    {
                        Position = OsuPlayfield.BASE_SIZE / 2,
                        StartTime = 5000,
                    },
                },
                Breaks =
                {
                    new BreakPeriod(2000, 3000)
                }
            };

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(testOnScreenDisplay = new TestOnScreenDisplay());
            Dependencies.CacheAs<OnScreenDisplay>(testOnScreenDisplay);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("reset OSD toast count", () => testOnScreenDisplay.ToastCount = 0);
        }

        [Test]
        public void TestFirstObjectTouched()
        {
            AddUntilStep("wait until 0 near", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(0).Within(500));
            AddStep("slow down", () => Player.GameplayClockContainer.AdjustmentsFromMods.Frequency.Value = 0.2);
            AddUntilStep("wait until 0", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(0));
            AddStep("touch circle", () =>
            {
                var touch = new Touch(TouchSource.Touch1, Player.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });
            AddAssert("touch device mod activated", () => Player.Score.ScoreInfo.Mods, () => Has.One.InstanceOf<OsuModTouchDevice>());
            AddAssert("no toasts displayed", () => testOnScreenDisplay.ToastCount, () => Is.Zero);
        }

        [Test]
        public void TestTouchDuringBreak()
        {
            AddUntilStep("wait until 2000 near", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(2000).Within(500));
            AddStep("slow down", () => Player.GameplayClockContainer.AdjustmentsFromMods.Frequency.Value = 0.2);
            AddUntilStep("wait until 2000", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(2000));
            AddStep("touch playfield", () =>
            {
                var touch = new Touch(TouchSource.Touch1, Player.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });
            AddAssert("touch device mod not activated", () => Player.Score.ScoreInfo.Mods, () => Has.None.InstanceOf<OsuModTouchDevice>());
            AddAssert("no toasts displayed", () => testOnScreenDisplay.ToastCount, () => Is.Zero);
        }

        [Test]
        public void TestSecondObjectTouched()
        {
            // ensure mouse is active (and that it's not suppressed due to touches in previous tests)
            AddStep("click mouse", () => InputManager.Click(MouseButton.Left));

            AddUntilStep("wait until 0 near", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(0).Within(500));
            AddStep("slow down", () => Player.GameplayClockContainer.AdjustmentsFromMods.Frequency.Value = 0.2);
            AddUntilStep("wait until 0", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(0));
            AddStep("click circle", () =>
            {
                InputManager.MoveMouseTo(Player.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("touch device mod not activated", () => Player.Mods.Value, () => Has.No.InstanceOf<OsuModTouchDevice>());

            AddStep("speed back up", () => Player.GameplayClockContainer.AdjustmentsFromMods.Frequency.Value = 1);
            AddUntilStep("wait until 5000 near", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(5000).Within(500));
            AddStep("slow down", () => Player.GameplayClockContainer.AdjustmentsFromMods.Frequency.Value = 0.2);
            AddUntilStep("wait until 5000", () => Player.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(5000));
            AddStep("touch playfield", () =>
            {
                var touch = new Touch(TouchSource.Touch1, Player.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });
            AddAssert("touch device mod activated", () => Player.Score.ScoreInfo.Mods, () => Has.One.InstanceOf<OsuModTouchDevice>());
            AddAssert("toast displayed", () => testOnScreenDisplay.ToastCount, () => Is.EqualTo(1));
        }

        private partial class TestOnScreenDisplay : OnScreenDisplay
        {
            public int ToastCount { get; set; }

            protected override void DisplayTemporarily(Drawable toDisplay)
            {
                base.DisplayTemporarily(toDisplay);
                ToastCount++;
            }
        }
    }
}
