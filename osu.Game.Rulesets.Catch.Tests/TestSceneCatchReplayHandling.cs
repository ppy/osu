// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Tests
{
    public partial class TestSceneCatchReplayHandling : OsuManualInputManagerTestScene
    {
        [Test]
        public void TestReplayDetach()
        {
            DrawableCatchRuleset drawableRuleset = null!;
            float catcherPosition = 0;

            AddStep("create drawable ruleset", () => Child = drawableRuleset = new DrawableCatchRuleset(new CatchRuleset(), new CatchBeatmap(), []));
            AddStep("attach replay", () => drawableRuleset.SetReplayScore(new Score()));
            AddStep("store catcher position", () => catcherPosition = drawableRuleset.ChildrenOfType<Catcher>().Single().X);
            AddStep("hold down left", () => InputManager.PressKey(Key.Left));
            AddAssert("catcher didn't move", () => drawableRuleset.ChildrenOfType<Catcher>().Single().X, () => Is.EqualTo(catcherPosition));
            AddStep("release left", () => InputManager.ReleaseKey(Key.Left));

            AddStep("detach replay", () => drawableRuleset.SetReplayScore(null));
            AddStep("hold down left", () => InputManager.PressKey(Key.Left));
            AddUntilStep("catcher moved", () => drawableRuleset.ChildrenOfType<Catcher>().Single().X, () => Is.Not.EqualTo(catcherPosition));
            AddStep("release left", () => InputManager.ReleaseKey(Key.Left));
        }
    }
}
