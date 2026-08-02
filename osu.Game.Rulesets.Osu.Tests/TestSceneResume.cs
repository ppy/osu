// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneResume : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(true, false, AllowBackwardsSeeks);

        [Test]
        public void TestPauseViaKeyboard()
        {
            AddStep("move mouse to center", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for gameplay start", () => Player.LocalUserPlaying.Value);
            AddStep("press escape", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("wait for pause overlay", () => Player.ChildrenOfType<PauseOverlay>().Single().State.Value, () => Is.EqualTo(Visibility.Visible));
            AddStep("release escape", () => InputManager.ReleaseKey(Key.Escape));
            AddStep("resume", () =>
            {
                InputManager.Key(Key.Down);
                InputManager.Key(Key.Space);
            });
            AddUntilStep("pause overlay present", () => Player.DrawableRuleset.ResumeOverlay.State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestPauseViaKeyboardWhenMouseOutsidePlayfield()
        {
            AddStep("move mouse outside playfield", () => InputManager.MoveMouseTo(Player.DrawableRuleset.Playfield.ScreenSpaceDrawQuad.BottomRight + new Vector2(1)));
            AddUntilStep("wait for gameplay start", () => Player.LocalUserPlaying.Value);
            AddStep("press escape", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("wait for pause overlay", () => Player.ChildrenOfType<PauseOverlay>().Single().State.Value, () => Is.EqualTo(Visibility.Visible));
            AddStep("release escape", () => InputManager.ReleaseKey(Key.Escape));
            AddStep("resume", () =>
            {
                InputManager.Key(Key.Down);
                InputManager.Key(Key.Space);
            });
            AddUntilStep("pause overlay present", () => Player.DrawableRuleset.ResumeOverlay.State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestPauseViaKeyboardWhenMouseOutsideScreen()
        {
            AddStep("move mouse outside playfield", () => InputManager.MoveMouseTo(new Vector2(-20)));
            AddUntilStep("wait for gameplay start", () => Player.LocalUserPlaying.Value);
            AddStep("press escape", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("wait for pause overlay", () => Player.ChildrenOfType<PauseOverlay>().Single().State.Value, () => Is.EqualTo(Visibility.Visible));
            AddStep("release escape", () => InputManager.ReleaseKey(Key.Escape));
            AddStep("resume", () =>
            {
                InputManager.Key(Key.Down);
                InputManager.Key(Key.Space);
            });
            AddUntilStep("pause overlay not present", () => Player.DrawableRuleset.ResumeOverlay.State.Value, () => Is.EqualTo(Visibility.Hidden));
        }
    }
}
