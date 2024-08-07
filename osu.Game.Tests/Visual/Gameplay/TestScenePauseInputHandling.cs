// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
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
            checkKey(() => counter, 0, false);

            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            checkKey(() => counter, 1, true);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKey(() => counter, 1, false);

            AddStep("pause", () => Player.Pause());
            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            checkKey(() => counter, 1, false);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKey(() => counter, 1, false);

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            checkKey(() => counter, 2, true);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKey(() => counter, 2, false);

            AddStep("press Z", () => InputManager.PressKey(Key.Z));
            checkKey(() => counter, 3, true);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKey(() => counter, 3, false);
        }

        [Test]
        public void TestManiaInputNotReceivedWhilePaused()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new ManiaRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<ManiaAction> actionTrigger && actionTrigger.Action == ManiaAction.Key4));
            checkKey(() => counter, 0, false);

            AddStep("press space", () => InputManager.PressKey(Key.Space));
            checkKey(() => counter, 1, true);

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            checkKey(() => counter, 1, false);

            AddStep("pause", () => Player.Pause());
            AddStep("press space", () => InputManager.PressKey(Key.Space));
            checkKey(() => counter, 1, false);

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            checkKey(() => counter, 1, false);

            AddStep("resume", () => Player.Resume());
            AddUntilStep("wait for resume", () => Player.GameplayClockContainer.IsRunning);

            AddStep("press space", () => InputManager.PressKey(Key.Space));
            checkKey(() => counter, 2, true);

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            checkKey(() => counter, 2, false);
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
            checkKey(() => counterZ, 1, false);

            AddStep("press X", () => InputManager.PressKey(Key.X));
            AddStep("pause", () => Player.Pause());
            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            checkKey(() => counterX, 1, true);

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            checkKey(() => counterZ, 2, true);
            checkKey(() => counterX, 1, false);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKey(() => counterZ, 2, false);
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
            checkKey(() => counter, 1, true);

            AddStep("resume", () => Player.Resume());
            AddUntilStep("wait for resume", () => Player.GameplayClockContainer.IsRunning);
            checkKey(() => counter, 1, false);
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
            checkKey(() => counterZ, 1, true);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKey(() => counterZ, 1, false);

            AddStep("press X", () => InputManager.PressKey(Key.X));
            checkKey(() => counterX, 1, true);

            AddStep("pause", () => Player.Pause());

            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            AddStep("press X", () => InputManager.PressKey(Key.X));

            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));
            checkKey(() => counterZ, 2, true);
            checkKey(() => counterX, 1, true);

            AddStep("release X", () => InputManager.ReleaseKey(Key.X));
            checkKey(() => counterX, 1, false);

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));
            checkKey(() => counterZ, 2, false);
        }

        [Test]
        public void TestManiaHeldInputRemainHeldAfterResume()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new ManiaRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<ManiaAction> actionTrigger && actionTrigger.Action == ManiaAction.Key4));

            AddStep("press space", () => InputManager.PressKey(Key.Space));
            checkKey(() => counter, 1, true);

            AddStep("pause", () => Player.Pause());

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            AddStep("press space", () => InputManager.PressKey(Key.Space));

            AddStep("resume", () => Player.Resume());
            AddUntilStep("wait for resume", () => Player.GameplayClockContainer.IsRunning);
            checkKey(() => counter, 1, true);

            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            checkKey(() => counter, 1, false);
        }

        [Test]
        public void TestOsuRegisterInputFromPressingOrangeCursorButPressIsBlocked()
        {
            KeyCounter counter = null!;

            loadPlayer(() => new OsuRuleset());
            AddStep("get key counter", () => counter = this.ChildrenOfType<KeyCounter>().Single(k => k.Trigger is KeyCounterActionTrigger<OsuAction> actionTrigger && actionTrigger.Action == OsuAction.LeftButton));

            AddStep("pause", () => Player.Pause());
            AddStep("resume", () => Player.Resume());
            AddStep("go to resume cursor", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single()));
            AddStep("press Z to resume", () => InputManager.PressKey(Key.Z));

            // ensure the input manager receives the Z button press...
            checkKey(() => counter, 1, true);
            AddAssert("button is pressed in kbc", () => Player.DrawableRuleset.Playfield.FindClosestParent<OsuInputManager>()!.PressedActions.Single() == OsuAction.LeftButton);

            // ...but also ensure the hit circle in front of the cursor isn't hit by checking max combo.
            AddAssert("circle not hit", () => Player.ScoreProcessor.HighestCombo.Value, () => Is.EqualTo(0));

            AddStep("release Z", () => InputManager.ReleaseKey(Key.Z));

            checkKey(() => counter, 1, false);
            AddAssert("button is released in kbc", () => !Player.DrawableRuleset.Playfield.FindClosestParent<OsuInputManager>()!.PressedActions.Any());
        }

        private void loadPlayer(Func<Ruleset> createRuleset)
        {
            AddStep("set ruleset", () => currentRuleset = createRuleset());
            AddStep("load player", LoadPlayer);
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
            AddUntilStep("wait for hud", () => Player.HUDOverlay.ChildrenOfType<SkinComponentsContainer>().All(s => s.ComponentsLoaded));

            AddStep("seek to gameplay", () => Player.GameplayClockContainer.Seek(0));
            AddUntilStep("wait for seek to finish", () => Player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(0).Within(500));
            AddAssert("not in break", () => !Player.IsBreakTime.Value);
            AddStep("move cursor to center", () => InputManager.MoveMouseTo(Player.DrawableRuleset.Playfield));
        }

        private void checkKey(Func<KeyCounter> counter, int count, bool active)
        {
            AddAssert($"key count = {count}", () => counter().CountPresses.Value, () => Is.EqualTo(count));
            AddAssert($"key active = {active}", () => counter().IsActive.Value, () => Is.EqualTo(active));
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
