// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    /// <summary>
    /// Test editor hotkeys at a high level to ensure they all work well together.
    /// </summary>
    public partial class TestSceneEditorBindings : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestBeatDivisorChangeHotkeys()
        {
            AddStep("hold shift", () => InputManager.PressKey(Key.LShift));

            AddStep("press 4", () => InputManager.Key(Key.Number4));
            AddAssert("snap updated to 4", () => EditorBeatmap.BeatmapInfo.BeatDivisor, () => Is.EqualTo(4));

            AddStep("press 6", () => InputManager.Key(Key.Number6));
            AddAssert("snap updated to 6", () => EditorBeatmap.BeatmapInfo.BeatDivisor, () => Is.EqualTo(6));

            AddStep("release shift", () => InputManager.ReleaseKey(Key.LShift));
        }
    }
}
