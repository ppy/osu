// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneOsuObjectPlacement : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestCannotSelectObjectsMidSpinnerPlacement()
        {
            AddStep("seek to first object", () => EditorClock.Seek(EditorBeatmap.HitObjects.First().StartTime));
            AddStep("select spinner placement tool", () => InputManager.Key(Key.Number4));
            AddStep("move mouse to composer", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuHitObjectComposer>().Single()));
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            AddStep("move mouse to slider", () => InputManager.MoveMouseTo(this.ChildrenOfType<SliderSelectionBlueprint>()
                                                                               .Single(b => b.HitObject == EditorBeatmap.HitObjects[0])
                                                                               .ScreenSpaceSelectionPoint));
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            AddAssert("placement still ongoing", () => EditorBeatmap.PlacementObject, () => Is.Not.Null);
        }
    }
}
