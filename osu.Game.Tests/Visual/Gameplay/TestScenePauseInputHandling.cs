// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Storyboards;
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
                new HitCircle
                {
                    Position = OsuPlayfield.BASE_SIZE / 2,
                    StartTime = 10000,
                },
                new HitCircle
                {
                    Position = OsuPlayfield.BASE_SIZE / 2,
                    StartTime = 15000,
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
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button never pressed in replay", f => f.Actions.Contains(OsuAction.LeftButton), 0);

            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counter, 1, true);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed once in replay", f => f.Actions.Contains(OsuAction.LeftButton), 1);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 1, false);

            AddStep("pause", () => Player.Pause());
            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counter, 1, false);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed once in replay", f => f.Actions.Contains(OsuAction.LeftButton), 1);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 1, false);

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counter, 2, true);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed twice in replay", f => f.Actions.Contains(OsuAction.LeftButton), 2);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 2, false);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed twice in replay", f => f.Actions.Contains(OsuAction.LeftButton), 2);

            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counter, 3, true);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed thrice in replay", f => f.Actions.Contains(OsuAction.LeftButton), 3);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 3, false);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed thrice in replay", f => f.Actions.Contains(OsuAction.LeftButton), 3);
        }

        [Test]
        public void TestManiaInputNotReceivedWhilePaused()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new ManiaRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<ManiaAction> actionTrigger && actionTrigger.Action == ManiaAction.Key4));
            checkKeyCounterState(() => counter, 0, false);
            assertCountOfMatchingReplayFrames<ManiaReplayFrame>("key4 never pressed in replay", f => f.Actions.Contains(ManiaAction.Key4), 0);

            AddStep("press space", () => InputManager.PressKey(Key.Space));
            checkKeyCounterState(() => counter, 1, true);
            assertCountOfMatchingReplayFrames<ManiaReplayFrame>("key4 pressed once in replay", f => f.Actions.Contains(ManiaAction.Key4), 1);

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            checkKeyCounterState(() => counter, 1, false);
            assertCountOfMatchingReplayFrames<ManiaReplayFrame>("key4 pressed once in replay", f => f.Actions.Contains(ManiaAction.Key4), 1);

            AddStep("pause", () => Player.Pause());
            AddStep("press space", () => InputManager.PressKey(Key.Space));
            checkKeyCounterState(() => counter, 1, false);
            assertCountOfMatchingReplayFrames<ManiaReplayFrame>("key4 pressed once in replay", f => f.Actions.Contains(ManiaAction.Key4), 1);

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            checkKeyCounterState(() => counter, 1, false);
            assertCountOfMatchingReplayFrames<ManiaReplayFrame>("key4 pressed once in replay", f => f.Actions.Contains(ManiaAction.Key4), 1);

            AddStep("resume", () => Player.Resume());
            AddUntilStep("wait for resume", () => Player.GameplayClockContainer.IsRunning);

            AddStep("press space", () => InputManager.PressKey(Key.Space));
            checkKeyCounterState(() => counter, 2, true);
            assertCountOfMatchingReplayFrames<ManiaReplayFrame>("key4 pressed twice in replay", f => f.Actions.Contains(ManiaAction.Key4), 2);

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            checkKeyCounterState(() => counter, 2, false);
            assertCountOfMatchingReplayFrames<ManiaReplayFrame>("key4 pressed twice in replay", f => f.Actions.Contains(ManiaAction.Key4), 2);
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
            AddStep("pause", () => Player.Pause());

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press and release Z", () => InputManager.Key(Key.Z));
            checkKeyCounterState(() => counterZ, 1, false);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed once in replay", f => f.Actions.Contains(OsuAction.LeftButton), 1);

            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddStep("pause", () => Player.Pause());
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            checkKeyCounterState(() => counterX, 1, true);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("right button pressed once in replay", f => f.Actions.Contains(OsuAction.RightButton), 1);

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            checkKeyCounterState(() => counterZ, 2, true);
            checkKeyCounterState(() => counterX, 1, false);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed twice in replay", f => f.Actions.Contains(OsuAction.LeftButton), 2);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("right button pressed once in replay", f => f.Actions.Contains(OsuAction.RightButton), 1);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counterZ, 2, false);
        }

        [Test]
        public void TestManiaPreviouslyHeldInputReleaseOnResume()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new ManiaRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<ManiaAction> actionTrigger && actionTrigger.Action == ManiaAction.Key4));

            AddStep("press space", () => InputManager.PressKey(Key.Space));
            AddStep("pause", () => Player.Pause());

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
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

            AddStep("press space", () => InputManager.PressKey(Key.Space));
            checkKeyCounterState(() => counter, 1, true);

            AddStep("pause", () => Player.Pause());

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            AddStep("press space", () => InputManager.PressKey(Key.Space));

            AddStep("resume", () => Player.Resume());
            AddUntilStep("wait for resume", () => Player.GameplayClockContainer.IsRunning);
            checkKeyCounterState(() => counter, 1, true);

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
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
            AddAssert("circle not hit", () => Player.ScoreProcessor.HighestCombo.Value, () => Is.EqualTo(0));
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button never pressed in replay", f => f.Actions.Contains(OsuAction.LeftButton), 0);

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
            AddAssert("circle hit", () => Player.ScoreProcessor.HighestCombo.Value, () => Is.EqualTo(1));
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed once in replay", f => f.Actions.Contains(OsuAction.LeftButton), 1);

            AddStep("pause", () => Player.Pause());
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));

            checkKeyCounterState(() => counter, 1, false);
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed once in replay", f => f.Actions.Contains(OsuAction.LeftButton), 1);

            seekTo(5000);

            AddStep("press Z", () => InputManager.PressKey(Key.Z));

            checkKeyCounterState(() => counter, 2, true);
            AddAssert("circle hit", () => Player.ScoreProcessor.HighestCombo.Value, () => Is.EqualTo(2));
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button pressed twice in replay", f => f.Actions.Contains(OsuAction.LeftButton), 2);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKeyCounterState(() => counter, 2, false);
        }

        [Test]
        public void TestOsuHitCircleNotReceivingInputOnResume_PauseWhileHoldingOtherKey()
        {
            loadPlayer(() => new OsuRuleset());

            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddAssert("circle hit", () => Player.ScoreProcessor.HighestCombo.Value, () => Is.EqualTo(1));
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("right button pressed once in replay", f => f.Actions.Contains(OsuAction.RightButton), 1);

            seekTo(5000);

            AddStep("pause", () => Player.Pause());
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));

            AddAssert("circle not hit", () => Player.ScoreProcessor.HighestCombo.Value, () => Is.EqualTo(1));
            assertCountOfMatchingReplayFrames<OsuReplayFrame>("left button never pressed in replay", f => f.Actions.Contains(OsuAction.LeftButton), 0);

            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));

            AddAssert("circle hit", () => Player.ScoreProcessor.HighestCombo.Value, () => Is.EqualTo(2));
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

        private void assertCountOfMatchingReplayFrames<TReplayFrame>(string description, Func<TReplayFrame, bool> predicate, int count)
            where TReplayFrame : ReplayFrame
        {
            AddAssert(description, () => Player.Score.Replay.Frames.OfType<TReplayFrame>().Count(predicate), () => Is.EqualTo(count));
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
