// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.Tests.Editor
{
    public partial class TestSceneEditorPlacement : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new TaikoRuleset();

        [Test]
        public void TestPlacementBlueprintDoesNotCauseCrashes()
        {
            AddStep("clear objects", () => EditorBeatmap.Clear());
            AddStep("add two objects", () =>
            {
                EditorBeatmap.Add(new Hit { StartTime = 1818 });
                EditorBeatmap.Add(new Hit { StartTime = 1584 });
            });
            AddStep("seek back", () => EditorClock.Seek(1584));
            AddStep("choose hit placement tool", () => InputManager.Key(Key.Number2));
            AddStep("hover over first hit", () => InputManager.MoveMouseTo(Editor.ChildrenOfType<DrawableHit>().ElementAt(1)));
            AddStep("hover over second hit", () => InputManager.MoveMouseTo(Editor.ChildrenOfType<DrawableHit>().ElementAt(0)));
            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddUntilStep("second hit deleted", () => Editor.ChildrenOfType<DrawableHit>().Count(), () => Is.EqualTo(1));
        }
    }
}
