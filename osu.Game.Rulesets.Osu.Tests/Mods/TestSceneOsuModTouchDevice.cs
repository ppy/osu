// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Configuration;
using osu.Game.Input;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModTouchDevice : RateAdjustedBeatmapTestScene
    {
        [Resolved]
        private SessionStatics statics { get; set; } = null!;

        private ScoreAccessibleSoloPlayer currentPlayer = null!;
        private readonly ManualClock manualClock = new ManualClock { Rate = 1 };

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
            => new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(manualClock), Audio);

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new TouchInputInterceptor());
        }

        public override void SetUpSteps()
        {
            AddStep("reset static", () => statics.SetValue(Static.TouchInputActive, false));
            base.SetUpSteps();
        }

        [Test]
        public void TestUserAlreadyHasTouchDeviceActive()
        {
            loadPlayer();
            AddStep("set up touchscreen user", () =>
            {
                currentPlayer.Score.ScoreInfo.Mods = currentPlayer.Score.ScoreInfo.Mods.Append(new OsuModTouchDevice()).ToArray();
                statics.SetValue(Static.TouchInputActive, true);
            });

            AddStep("seek to 0", () => currentPlayer.GameplayClockContainer.Seek(0));
            AddUntilStep("wait until 0", () => currentPlayer.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(0));
            AddStep("touch circle", () =>
            {
                var touch = new Touch(TouchSource.Touch1, currentPlayer.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });
            AddAssert("touch device mod activated", () => currentPlayer.Score.ScoreInfo.Mods, () => Has.One.InstanceOf<OsuModTouchDevice>());
        }

        [Test]
        public void TestTouchActivePriorToPlayerLoad()
        {
            AddStep("set touch input active", () => statics.SetValue(Static.TouchInputActive, true));
            loadPlayer();
            AddUntilStep("touch device mod activated", () => currentPlayer.Score.ScoreInfo.Mods, () => Has.One.InstanceOf<OsuModTouchDevice>());
        }

        [Test]
        public void TestTouchDuringBreak()
        {
            loadPlayer();
            AddStep("seek to 2000", () => currentPlayer.GameplayClockContainer.Seek(2000));
            AddUntilStep("wait until 2000", () => currentPlayer.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(2000));
            AddUntilStep("wait until break entered", () => currentPlayer.IsBreakTime.Value);
            AddStep("touch playfield", () =>
            {
                var touch = new Touch(TouchSource.Touch1, currentPlayer.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });
            AddAssert("touch device mod not activated", () => currentPlayer.Score.ScoreInfo.Mods, () => Has.None.InstanceOf<OsuModTouchDevice>());
        }

        [Test]
        public void TestTouchMiss()
        {
            loadPlayer();
            // ensure mouse is active (and that it's not suppressed due to touches in previous tests)
            AddStep("click mouse", () => InputManager.Click(MouseButton.Left));

            AddStep("seek to 200", () => currentPlayer.GameplayClockContainer.Seek(200));
            AddUntilStep("wait until 200", () => currentPlayer.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(200));
            AddStep("touch playfield", () =>
            {
                var touch = new Touch(TouchSource.Touch1, currentPlayer.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });
            AddAssert("touch device mod activated", () => currentPlayer.Score.ScoreInfo.Mods, () => Has.One.InstanceOf<OsuModTouchDevice>());
        }

        [Test]
        public void TestIncompatibleModActive()
        {
            loadPlayer();
            // this is only a veneer of enabling autopilot as having it actually active from the start is annoying to make happen
            // given the tests' structure.
            AddStep("enable autopilot", () => currentPlayer.Score.ScoreInfo.Mods = new Mod[] { new OsuModAutopilot() });

            AddStep("seek to 0", () => currentPlayer.GameplayClockContainer.Seek(0));
            AddUntilStep("wait until 0", () => currentPlayer.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(0));
            AddStep("touch playfield", () =>
            {
                var touch = new Touch(TouchSource.Touch1, currentPlayer.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });
            AddAssert("touch device mod not activated", () => currentPlayer.Score.ScoreInfo.Mods, () => Has.None.InstanceOf<OsuModTouchDevice>());
        }

        [Test]
        public void TestSecondObjectTouched()
        {
            loadPlayer();
            // ensure mouse is active (and that it's not suppressed due to touches in previous tests)
            AddStep("click mouse", () => InputManager.Click(MouseButton.Left));

            AddStep("seek to 0", () => currentPlayer.GameplayClockContainer.Seek(0));
            AddUntilStep("wait until 0", () => currentPlayer.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(0));
            AddStep("click circle", () =>
            {
                InputManager.MoveMouseTo(currentPlayer.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("touch device mod not activated", () => currentPlayer.Score.ScoreInfo.Mods, () => Has.None.InstanceOf<OsuModTouchDevice>());

            AddStep("seek to 5000", () => currentPlayer.GameplayClockContainer.Seek(5000));
            AddUntilStep("wait until 5000", () => currentPlayer.GameplayClockContainer.CurrentTime, () => Is.GreaterThanOrEqualTo(5000));
            AddStep("touch playfield", () =>
            {
                var touch = new Touch(TouchSource.Touch1, currentPlayer.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.Centre);
                InputManager.BeginTouch(touch);
                InputManager.EndTouch(touch);
            });
            AddAssert("touch device mod activated", () => currentPlayer.Score.ScoreInfo.Mods, () => Has.One.InstanceOf<OsuModTouchDevice>());
        }

        private void loadPlayer()
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new OsuBeatmap
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
                });

                var p = new ScoreAccessibleSoloPlayer();

                LoadScreen(currentPlayer = p);
            });

            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
        }

        private partial class ScoreAccessibleSoloPlayer : SoloPlayer
        {
            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public new DrawableRuleset DrawableRuleset => base.DrawableRuleset;

            protected override bool PauseOnFocusLost => false;

            public ScoreAccessibleSoloPlayer()
                : base(new PlayerConfiguration
                {
                    AllowPause = false,
                    ShowResults = false,
                })
            {
            }
        }
    }
}
