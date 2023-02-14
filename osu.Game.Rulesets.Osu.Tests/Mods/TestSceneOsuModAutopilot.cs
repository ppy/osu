// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModAutopilot : OsuModTestScene
    {
        [Test]
        public void TestInstantResume()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModAutopilot(),
                PassCondition = () => true,
                Autoplay = false,
            });

            AddUntilStep("wait for gameplay start", () => Player.LocalUserPlaying.Value);
            AddStep("press pause", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("wait until paused", () => Player.GameplayClockContainer.IsPaused.Value);
            AddStep("release pause", () => InputManager.ReleaseKey(Key.Escape));
            AddStep("press resume", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("wait for resume", () => !Player.IsResuming);
            AddAssert("resumed", () => !Player.GameplayClockContainer.IsPaused.Value);
        }
    }
}
