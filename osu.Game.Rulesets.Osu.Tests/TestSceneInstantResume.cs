// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneInstantResume : TestSceneOsuPlayer
    {
        protected override bool HasCustomSteps => true;

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { new OsuModAutopilot() };
            return new TestPlayer();
        }

        [Test]
        public void TestInstantResume()
        {
            CreateTest();

            AddStep("press pause", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("wait until paused", () => Player.GameplayClockContainer.IsPaused.Value);
            AddStep("release pause", () => InputManager.ReleaseKey(Key.Escape));
            AddStep("press resume", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("wait for resume", () => !Player.IsResuming);
            AddAssert("resumed", () => !Player.GameplayClockContainer.IsPaused.Value);
        }
    }
}
