// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestSceneManiaSelectionHandler : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new ManiaRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [Test]
        public void TestHorizontalFlipOverSelection()
        {
            ManiaHitObject first = null!, second = null!, third = null!;

            AddStep("create objects", () =>
            {
                EditorBeatmap.Add(first = new Note { StartTime = 250, Column = 2 });
                EditorBeatmap.Add(second = new HoldNote { StartTime = 750, Duration = 1500, Column = 1 });
                EditorBeatmap.Add(third = new Note { StartTime = 1250, Column = 3 });
            });

            AddStep("select everything", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            AddStep("flip horizontally over selection", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxButton>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("first object stayed in place", () => first.Column, () => Is.EqualTo(2));
            AddAssert("second object flipped", () => second.Column, () => Is.EqualTo(3));
            AddAssert("third object flipped", () => third.Column, () => Is.EqualTo(1));
        }

        [Test]
        public void TestHorizontalFlipOverPlayfield()
        {
            ManiaHitObject first = null!, second = null!, third = null!;

            AddStep("create objects", () =>
            {
                EditorBeatmap.Add(first = new Note { StartTime = 250, Column = 2 });
                EditorBeatmap.Add(second = new HoldNote { StartTime = 750, Duration = 1500, Column = 1 });
                EditorBeatmap.Add(third = new Note { StartTime = 1250, Column = 3 });
            });

            AddStep("select everything", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            AddStep("flip horizontally", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.H);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddAssert("first object flipped", () => first.Column, () => Is.EqualTo(1));
            AddAssert("second object flipped", () => second.Column, () => Is.EqualTo(2));
            AddAssert("third object flipped", () => third.Column, () => Is.EqualTo(0));
        }

        [Test]
        public void TestVerticalFlip()
        {
            ManiaHitObject first = null!, second = null!, third = null!;

            AddStep("create objects", () =>
            {
                EditorBeatmap.Add(first = new Note { StartTime = 250, Column = 2 });
                EditorBeatmap.Add(second = new HoldNote { StartTime = 750, Duration = 1500, Column = 1 });
                EditorBeatmap.Add(third = new Note { StartTime = 1250, Column = 3 });
            });

            AddStep("select everything", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            AddStep("flip vertically", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.J);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddAssert("first object flipped", () => first.StartTime, () => Is.EqualTo(2250));
            AddAssert("second object flipped", () => second.StartTime, () => Is.EqualTo(250));
            AddAssert("third object flipped", () => third.StartTime, () => Is.EqualTo(1250));
        }
    }
}
