// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestScenePauseInputHandling : PlayerTestScene
    {
        private Ruleset currentRuleset = new OsuRuleset();

        protected override Ruleset CreatePlayerRuleset() => currentRuleset;

        protected override bool HasCustomSteps => true;

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
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
                new Slider
                {
                    Position = OsuPlayfield.BASE_SIZE / 2,
                    StartTime = 10000,
                    Path = new SliderPath
                    {
                        ControlPoints =
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(0, 100))
                        }
                    },
                    Samples = [new HitSampleInfo(HitSampleInfo.HIT_NORMAL)]
                },
                new Spinner
                {
                    Position = OsuPlayfield.BASE_SIZE / 2,
                    StartTime = 15000,
                    Duration = 5000,
                }
            }
        };

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null) =>
            new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            foreach (var key in InputManager.CurrentState.Keyboard.Keys)
                InputManager.ReleaseKey(key);

            InputManager.MoveMouseTo(Content);
            LocalConfig.SetValue(OsuSetting.KeyOverlay, true);
        });

        [Test]
        public void TestOsuInputNotReceivedWhilePaused()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new OsuRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<OsuAction> actionTrigger && actionTrigger.Action == OsuAction.LeftButton));
            checkKeyCounterState(() => counter, 0, false);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 0);

            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counter, 1, true);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 1);

            seekTo(50);
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 1, false);

            seekTo(100);
            AddStep("pause", () => Player.Pause());
            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counter, 1, false);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 1);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 1, false);

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counter, 2, true);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 2);

            seekTo(150);
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 2, false);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 2);

            seekTo(200);
            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counter, 3, true);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 3);

            seekTo(250);
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 3, false);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 3);
        }

        [Test]
        public void TestManiaInputNotReceivedWhilePaused()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new ManiaRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<ManiaAction> actionTrigger && actionTrigger.Action == ManiaAction.Key4));
            checkKeyCounterState(() => counter, 0, false);
            assertCountOfPressesInManiaReplay(ManiaAction.Key4, 0);

            AddStep("press J", () => InputManager.PressKey(Key.J));
            checkKeyCounterState(() => counter, 1, true);
            assertCountOfPressesInManiaReplay(ManiaAction.Key4, 1);

            seekTo(50);
            AddStep("release J", () => InputManager.ReleaseKey(Key.J));
            checkKeyCounterState(() => counter, 1, false);
            assertCountOfPressesInManiaReplay(ManiaAction.Key4, 1);

            seekTo(100);
            AddStep("pause", () => Player.Pause());
            AddStep("press J", () => InputManager.PressKey(Key.J));
            checkKeyCounterState(() => counter, 1, false);
            assertCountOfPressesInManiaReplay(ManiaAction.Key4, 1);

            seekTo(150);
            AddStep("release J", () => InputManager.ReleaseKey(Key.J));
            checkKeyCounterState(() => counter, 1, false);
            assertCountOfPressesInManiaReplay(ManiaAction.Key4, 1);

            seekTo(200);
            AddStep("resume", () => Player.Resume());
            AddUntilStep("wait for resume", () => Player.GameplayClockContainer.IsRunning);

            AddStep("press J", () => InputManager.PressKey(Key.J));
            checkKeyCounterState(() => counter, 2, true);
            assertCountOfPressesInManiaReplay(ManiaAction.Key4, 2);

            seekTo(250);
            AddStep("release J", () => InputManager.ReleaseKey(Key.J));
            checkKeyCounterState(() => counter, 2, false);
            assertCountOfPressesInManiaReplay(ManiaAction.Key4, 2);
        }

        [Test]
        public void TestOsuPreviouslyHeldInputReleaseOnResume()
        {
            KeyCounter counterZ = null!;
            KeyCounter counterX = null!;

            loadPlayer(() => new OsuRuleset());
            AddStep("get key counter Z", () => counterZ = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<OsuAction> actionTrigger && actionTrigger.Action == OsuAction.LeftButton));
            AddStep("get key counter X", () => counterX = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<OsuAction> actionTrigger && actionTrigger.Action == OsuAction.RightButton));

            AddStep("press Z", () => InputManager.PressKey(Key.Z));

            seekTo(50);
            AddStep("pause", () => Player.Pause());
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counterZ, 1, true);

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press and release Z", () => InputManager.Key(Key.Z));
            checkKeyCounterState(() => counterZ, 1, false);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 1);

            seekTo(100);
            AddStep("press X", () => InputManager.PressKey(Key.X));
            checkKeyCounterState(() => counterX, 1, true);
            assertCountOfPressesInOsuReplay(OsuAction.RightButton, 1);

            seekTo(150);
            AddStep("pause", () => Player.Pause());
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            checkKeyCounterState(() => counterX, 1, true);

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counterZ, 2, true);
            checkKeyCounterState(() => counterX, 1, false);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 2);
            assertCountOfPressesInOsuReplay(OsuAction.RightButton, 1);

            seekTo(200);
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counterZ, 2, false);
        }

        [Test]
        public void TestManiaPreviouslyHeldInputReleaseOnResume()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new ManiaRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<ManiaAction> actionTrigger && actionTrigger.Action == ManiaAction.Key4));

            AddStep("press J", () => InputManager.PressKey(Key.J));
            AddStep("pause", () => Player.Pause());

            AddStep("release J", () => InputManager.ReleaseKey(Key.J));
            checkKeyCounterState(() => counter, 1, true);

            AddStep("resume", () => Player.Resume());
            AddUntilStep("wait for resume", () => Player.GameplayClockContainer.IsRunning);
            checkKeyCounterState(() => counter, 1, false);
        }

        [Test]
        public void TestOsuHeldInputRemainHeldAfterResume()
        {
            KeyCounter counterZ = null!;
            KeyCounter counterX = null!;

            loadPlayer(() => new OsuRuleset());
            AddStep("get key counter Z", () => counterZ = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<OsuAction> actionTrigger && actionTrigger.Action == OsuAction.LeftButton));
            AddStep("get key counter X", () => counterX = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<OsuAction> actionTrigger && actionTrigger.Action == OsuAction.RightButton));

            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            AddStep("pause", () => Player.Pause());

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counterZ, 1, true);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counterZ, 1, false);

            AddStep("press X", () => InputManager.PressKey(Key.X));
            checkKeyCounterState(() => counterX, 1, true);

            AddStep("pause", () => Player.Pause());

            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            AddStep("press X", () => InputManager.PressKey(Key.X));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counterZ, 2, true);
            checkKeyCounterState(() => counterX, 1, true);

            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            checkKeyCounterState(() => counterX, 1, false);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counterZ, 2, false);
        }

        [Test]
        public void TestManiaHeldInputRemainHeldAfterResume()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new ManiaRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<ManiaAction> actionTrigger && actionTrigger.Action == ManiaAction.Key4));

            AddStep("press J", () => InputManager.PressKey(Key.J));
            checkKeyCounterState(() => counter, 1, true);

            AddStep("pause", () => Player.Pause());

            AddStep("release J", () => InputManager.ReleaseKey(Key.J));
            AddStep("press J", () => InputManager.PressKey(Key.J));

            AddStep("resume", () => Player.Resume());
            AddUntilStep("wait for resume", () => Player.GameplayClockContainer.IsRunning);
            checkKeyCounterState(() => counter, 1, true);

            AddStep("release J", () => InputManager.ReleaseKey(Key.J));
            checkKeyCounterState(() => counter, 1, false);
        }

        [Test]
        public void TestOsuHitCircleNotReceivingInputOnResume()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new OsuRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<OsuAction> actionTrigger && actionTrigger.Action == OsuAction.LeftButton));

            AddStep("pause", () => Player.Pause());
            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));

            checkKeyCounterState(() => counter, 0, false);
            AddAssert("circle not hit", () => Player.ScoreProcessor.Combo.Value, () => Is.EqualTo(0));
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 0);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 0, false);
        }

        [Test]
        public void TestOsuHitCircleNotReceivingInputOnResume_PauseWhileHoldingSameKey()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new OsuRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<OsuAction> actionTrigger && actionTrigger.Action == OsuAction.LeftButton));

            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            AddAssert("circle hit", () => Player.ScoreProcessor.Combo.Value, () => Is.EqualTo(1));
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 1);

            seekTo(50);
            AddStep("pause", () => Player.Pause());
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press and release Z to resume", () => InputManager.Key(Key.Z));

            checkKeyCounterState(() => counter, 1, false);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 1);

            seekTo(5000);

            AddStep("press Z", () => InputManager.PressKey(Key.Z));

            checkKeyCounterState(() => counter, 2, true);
            AddAssert("circle hit", () => Player.ScoreProcessor.Combo.Value, () => Is.EqualTo(2));
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 2);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 2, false);
        }

        [Test]
        public void TestOsuHitCircleNotReceivingInputOnResume_PauseWhileHoldingOtherKey()
        {
            loadPlayer(() => new OsuRuleset());

            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddAssert("circle hit", () => Player.ScoreProcessor.Combo.Value, () => Is.EqualTo(1));
            assertCountOfPressesInOsuReplay(OsuAction.RightButton, 1);

            seekTo(5000);

            AddStep("pause", () => Player.Pause());
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));

            AddAssert("circle not hit", () => Player.ScoreProcessor.Combo.Value, () => Is.EqualTo(1));
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 0);

            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));

            AddAssert("circle hit", () => Player.ScoreProcessor.Combo.Value, () => Is.EqualTo(2));
        }

        [Test]
        public void TestOsuSliderContinuesTrackingOnResume([Values] bool resumeWithSameKey)
        {
            loadPlayer(() => new OsuRuleset());

            seekTo(10000);
            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddAssert("slider head hit", () => Player.ScoreProcessor.Combo.Value, () => Is.EqualTo(1));
            AddAssert("slider tracking", () => this.ChildrenOfType<SliderInputManager>().Single().Tracking, () => Is.True);

            // note operation ordering - gameplay paused while still holding X
            AddStep("pause", () => Player.Pause());
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));

            if (resumeWithSameKey)
                AddStep("press X", () => InputManager.PressKey(Key.X));
            else
                AddStep("press Z", () => InputManager.PressKey(Key.Z));

            // there's a nasty interaction with `SliderInputManager`'s "time to accept any key after" mechanic here.
            // basically switching keys when holding a slider is only permitted *after* the head was hit with one key and the other is not pressed.
            // if the delta between the seek above and this one is too small, the current time of player
            // may not change between the head hit and the slider tracking switch (because of frame stability),
            // therefore this test fails in the "resume with other key" scenario due to assuming the key switch is illegal at that time.
            // thus, the delta between the seek above and this one must be large enough to make that improbable to occur.
            seekTo(10040);
            AddAssert("slider still tracking", () => this.ChildrenOfType<SliderInputManager>().Single().Tracking, () => Is.True);

            if (resumeWithSameKey)
                AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            else
                AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
        }

        [Test]
        public void TestOsuSliderPicksUpTrackingOnResume([Values] bool resumeWithSameKey)
        {
            loadPlayer(() => new OsuRuleset());

            seekTo(10000);
            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddAssert("slider head hit", () => Player.ScoreProcessor.Combo.Value, () => Is.EqualTo(1));
            AddAssert("slider tracking", () => this.ChildrenOfType<SliderInputManager>().Single().Tracking, () => Is.True);

            // note operation ordering - gameplay paused while not holding anything
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            AddAssert("slider not tracking", () => this.ChildrenOfType<SliderInputManager>().Single().Tracking, () => Is.False);
            AddStep("pause", () => Player.Pause());

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));

            if (resumeWithSameKey)
                AddStep("press X", () => InputManager.PressKey(Key.X));
            else
                AddStep("press Z", () => InputManager.PressKey(Key.Z));

            // there's a nasty interaction with `SliderInputManager`'s "time to accept any key after" mechanic here.
            // basically switching keys when holding a slider is only permitted *after* the head was hit with one key and the other is not pressed.
            // if the delta between the seek above and this one is too small, the current time of player
            // may not change between the head hit and the slider tracking switch (because of frame stability),
            // therefore this test fails in the "resume with other key" scenario due to assuming the key switch is illegal at that time.
            // thus, the delta between the seek above and this one must be large enough to make that improbable to occur.
            seekTo(10040);
            AddAssert("slider tracking again", () => this.ChildrenOfType<SliderInputManager>().Single().Tracking, () => Is.True);

            if (resumeWithSameKey)
                AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            else
                AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
        }

        [Test]
        public void TestSliderCanStartTrackingFromResumePress()
        {
            loadPlayer(() => new OsuRuleset());

            seekTo(10000);
            AddStep("pause", () => Player.Pause());

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));

            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            AddAssert("slider head hit", () => this.ChildrenOfType<DrawableSliderHead>().Single().IsHit, () => Is.True);
            AddAssert("slider tracking", () => this.ChildrenOfType<SliderInputManager>().Single().Tracking, () => Is.True);
            assertCountOfPressesInOsuReplay(OsuAction.LeftButton, 1);

            seekTo(10040);
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
        }

        [Test]
        public void TestOsuSpinnerContinuesTrackingOnResume([Values] bool resumeWithSameKey)
        {
            loadPlayer(() => new OsuRuleset());

            seekTo(15000);
            AddStep("move mouse higher", () => InputManager.MoveMouseTo(Content, new Vector2(0, -50)));
            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddAssert("spinner tracking", () => this.ChildrenOfType<SpinnerRotationTracker>().Single().Tracking, () => Is.True);

            // note operation ordering - gameplay paused while still holding X
            AddStep("pause", () => Player.Pause());
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));

            if (resumeWithSameKey)
                AddStep("press X", () => InputManager.PressKey(Key.X));
            else
                AddStep("press Z", () => InputManager.PressKey(Key.Z));

            seekTo(15040);
            AddAssert("spinner still tracking", () => this.ChildrenOfType<SpinnerRotationTracker>().Single().Tracking, () => Is.True);

            if (resumeWithSameKey)
                AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            else
                AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
        }

        [Test]
        public void TestOsuSpinnerPicksUpTrackingOnResume([Values] bool resumeWithSameKey)
        {
            loadPlayer(() => new OsuRuleset());

            seekTo(15000);
            AddStep("move mouse higher", () => InputManager.MoveMouseTo(Content, new Vector2(0, -50)));
            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddAssert("spinner tracking", () => this.ChildrenOfType<SpinnerRotationTracker>().Single().Tracking, () => Is.True);

            // note operation ordering - gameplay paused while not holding anything
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            AddAssert("spinner not tracking", () => this.ChildrenOfType<SpinnerRotationTracker>().Single().Tracking, () => Is.False);
            AddStep("pause", () => Player.Pause());

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));

            if (resumeWithSameKey)
                AddStep("press X", () => InputManager.PressKey(Key.X));
            else
                AddStep("press Z", () => InputManager.PressKey(Key.Z));

            seekTo(15040);
            AddAssert("spinner tracking again", () => this.ChildrenOfType<SpinnerRotationTracker>().Single().Tracking, () => Is.True);

            if (resumeWithSameKey)
                AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            else
                AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
        }

        private void loadPlayer(Func<Ruleset> createRuleset)
        {
            AddStep("set ruleset", () => currentRuleset = createRuleset());
            AddStep("load player", LoadPlayer);
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
            AddUntilStep("wait for hud", () => Player.HUDOverlay.ChildrenOfType<SkinnableContainer>().All(s => s.ComponentsLoaded));

            seekTo(0);
            AddAssert("not in break", () => !Player.IsBreakTime.Value);
            AddStep("move cursor to center", () => InputManager.MoveMouseTo(Player.DrawableRuleset.Playfield));
        }

        private void seekTo(double time)
        {
            AddStep($"seek to {time}ms", () => Player.GameplayClockContainer.Seek(time));
            AddUntilStep("wait for seek to finish", () => Player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(500));
        }

        private void checkKeyCounterState(Func<KeyCounter> counter, int count, bool active)
        {
            AddAssert($"key count = {count}", () => counter().CountPresses.Value, () => Is.EqualTo(count));
            AddAssert($"key active = {active}", () => counter().IsActive.Value, () => Is.EqualTo(active));
        }

        private void assertCountOfPressesInOsuReplay(OsuAction action, int expectedCount)
        {
            AddUntilStep($"{action} pressed {expectedCount} time(s) in replay", () =>
            {
                int count = 0;
                bool pressed = false;

                foreach (var frame in Player.Score.Replay.Frames.Cast<OsuReplayFrame>())
                {
                    if (!frame.Actions.Contains(action))
                        pressed = false;
                    else
                    {
                        if (!pressed)
                        {
                            count++;
                            pressed = true;
                        }
                    }
                }

                return count;
            }, () => Is.EqualTo(expectedCount));
        }

        private void assertCountOfPressesInManiaReplay(ManiaAction action, int expectedCount)
        {
            AddUntilStep($"{action} pressed {expectedCount} time(s) in replay", () =>
            {
                int count = 0;
                bool pressed = false;

                foreach (var frame in Player.Score.Replay.Frames.Cast<ManiaReplayFrame>())
                {
                    if (!frame.Actions.Contains(action))
                        pressed = false;
                    else
                    {
                        if (!pressed)
                        {
                            count++;
                            pressed = true;
                        }
                    }
                }

                return count;
            }, () => Is.EqualTo(expectedCount));
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new PausePlayer();

        private partial class PausePlayer : TestPlayer
        {
            protected override double PauseCooldownDuration => 0;

            public PausePlayer()
                : base(allowPause: true, showResults: false)
            {
            }
        }
    }
}
