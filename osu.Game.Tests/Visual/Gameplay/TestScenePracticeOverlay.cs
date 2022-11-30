// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Overlays.Practice;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestScenePracticeOverlay : ScreenTestScene
    {
        private TestPracticePlayerLoader loader = null!;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(Realm);
        }

        private void resetPlayer(Action? beforeLoadAction = null)
        {
            beforeLoadAction?.Invoke();

            prepareBeatmap();

            LoadScreen(loader = new TestPracticePlayerLoader());
        }

        private void prepareBeatmap()
        {
            var workingBeatmap = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            // Add intro time to test quick retry skipping (TestQuickRetry).
            workingBeatmap.BeatmapInfo.AudioLeadIn = 60000;

            // Turn on epilepsy warning to test warning display (TestEpilepsyWarning).
            workingBeatmap.BeatmapInfo.EpilepsyWarning = false;

            Beatmap.Value = workingBeatmap;

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(Beatmap.Value.Track);
        }

        [Test]
        public void TestHideOnRestart()
        {
            AddStep("Reset player", () => resetPlayer(() => SelectedMods.Value = new[] { new OsuModNightcore() }));
            AddUntilStep("Player loaded", () => loader.Player.IsCurrentScreen());
            AddStep("Test restart", () => loader.Player.Restart());
            AddUntilStep("Player loaded", () => loader.Player.IsCurrentScreen());
            AddAssert("Assert overlay hidden", () => !loader.Player.PracticeOverlay.IsPresent);
        }

        [Test]
        public void TestRestartWithMouse()
        {
            AddStep("Reset player", () => resetPlayer(() => SelectedMods.Value = new[] { new OsuModNightcore() }));
            AddUntilStep("Player loaded", () => loader.Player.IsCurrentScreen());
            AddStep("Change custom start", () => loader.CustomStart.Value = 0.5f);
            AddStep("Click button", () =>
            {
                InputManager.MoveMouseTo(loader.Player.PracticeOverlay.RestartButton);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("Player loaded", () => loader.Player.IsCurrentScreen());
            AddAssert("custom start not default", () => loader.CustomStart.Value != loader.CustomStart.Default);
        }

        [Test]
        public void TestTogglePausePractice()
        {
            AddStep("Reset player", () => resetPlayer());
            AddUntilStep("Player loaded", () => loader.Player.IsCurrentScreen());
            AddStep("Toggle hide overlay", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("Overlay hidden", () => !loader.Player.PracticeOverlay.IsPresent);
            AddStep("Toggle", () => InputManager.PressKey(Key.Up));
            AddStep("Enter overlay", () => InputManager.PressKey(Key.Enter));
        }

        [Test]
        public void TestGracePeriodWithMods()
        {
            AddStep("Reset player", () => resetPlayer(() => SelectedMods.Value = new[] { new OsuModSuddenDeath() }));
            AddUntilStep("Player loaded", () => loader.Player.IsCurrentScreen());
            AddStep("Restart player", () => loader.Player.Restart());
            AddUntilStep("Player loaded", () => loader.Player.IsCurrentScreen());
            AddStep("Seek", () => loader.Player.Seek(19000));
            AddAssert("Test player alive", () => !loader.Player.GameplayState.HasFailed);
            AddUntilStep("Wait for failure", () => loader.Player.GameplayState.HasFailed);
            AddAssert("Check blockFail is false", () => loader.Player.BlockFail == false);
        }

        //Exposes stuff for testing
        private partial class TestPracticePlayer : PracticePlayer
        {
            public new bool BlockFail => base.BlockFail;

            public new PracticeOverlay PracticeOverlay => base.PracticeOverlay;

            public TestPracticePlayer(PracticePlayerLoader loader)
                : base(loader)
            {
            }
        }

        private partial class TestPracticePlayerLoader : PracticePlayerLoader
        {
            public TestPracticePlayer Player = null!;

            public TestPracticePlayerLoader()
            {
                CreatePlayer = () => Player = new TestPracticePlayer(this);
            }
        }
    }
}
