// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Visual;
using osu.Game.Utils;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneOsuEditorGrids : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestGridToggles()
        {
            AddStep("enable distance snap grid", () => InputManager.Key(Key.T));
            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));

            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            gridActive<RectangularPositionSnapGrid>(false);

            AddStep("enable rectangular grid", () => InputManager.Key(Key.Y));

            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));
            AddUntilStep("distance snap grid still visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            gridActive<RectangularPositionSnapGrid>(true);

            AddStep("disable distance snap grid", () => InputManager.Key(Key.T));
            AddUntilStep("distance snap grid hidden", () => !this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));
            gridActive<RectangularPositionSnapGrid>(true);

            AddStep("disable rectangular grid", () => InputManager.Key(Key.Y));
            AddUntilStep("distance snap grid still hidden", () => !this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            gridActive<RectangularPositionSnapGrid>(false);
        }

        [Test]
        public void TestDistanceSnapMomentaryToggle()
        {
            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));

            AddUntilStep("distance snap grid hidden", () => !this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddStep("hold alt", () => InputManager.PressKey(Key.AltLeft));
            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddStep("release alt", () => InputManager.ReleaseKey(Key.AltLeft));
            AddUntilStep("distance snap grid hidden", () => !this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
        }

        [Test]
        public void TestGridSnapMomentaryToggle()
        {
            gridActive<RectangularPositionSnapGrid>(false);
            AddStep("hold shift", () => InputManager.PressKey(Key.ShiftLeft));
            gridActive<RectangularPositionSnapGrid>(true);
            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));
            gridActive<RectangularPositionSnapGrid>(false);
        }

        private void gridActive<T>(bool active) where T : PositionSnapGrid
        {
            AddStep("choose placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move cursor to spacing + (1, 1)", () =>
            {
                var composer = Editor.ChildrenOfType<T>().Single();
                InputManager.MoveMouseTo(composer.ToScreenSpace(uniqueSnappingPosition(composer) + new Vector2(1, 1)));
            });

            if (active)
            {
                AddAssert("placement blueprint at spacing + (0, 0)", () =>
                {
                    var composer = Editor.ChildrenOfType<T>().Single();
                    return Precision.AlmostEquals(Editor.ChildrenOfType<HitCirclePlacementBlueprint>().Single().HitObject.Position,
                        uniqueSnappingPosition(composer));
                });
            }
            else
            {
                AddAssert("placement blueprint at spacing + (1, 1)", () =>
                {
                    var composer = Editor.ChildrenOfType<T>().Single();
                    return Precision.AlmostEquals(Editor.ChildrenOfType<HitCirclePlacementBlueprint>().Single().HitObject.Position,
                        uniqueSnappingPosition(composer) + new Vector2(1, 1));
                });
            }
        }

        private Vector2 uniqueSnappingPosition(PositionSnapGrid grid)
        {
            return grid switch
            {
                RectangularPositionSnapGrid rectangular => rectangular.StartPosition.Value + GeometryUtils.RotateVector(rectangular.Spacing.Value, -rectangular.GridLineRotation.Value),
                _ => Vector2.Zero
            };
        }

        [Test]
        public void TestGridSizeToggling()
        {
            AddStep("enable rectangular grid", () => InputManager.Key(Key.Y));
            AddUntilStep("rectangular grid visible", () => this.ChildrenOfType<RectangularPositionSnapGrid>().Any());
            gridSizeIs(4);

            nextGridSizeIs(8);
            nextGridSizeIs(16);
            nextGridSizeIs(32);
            nextGridSizeIs(4);
        }

        private void nextGridSizeIs(int size)
        {
            AddStep("toggle to next grid size", () => InputManager.Key(Key.G));
            gridSizeIs(size);
        }

        private void gridSizeIs(int size)
            => AddAssert($"grid size is {size}", () => this.ChildrenOfType<RectangularPositionSnapGrid>().Single().Spacing.Value == new Vector2(size)
                                                       && EditorBeatmap.BeatmapInfo.GridSize == size);
    }
}
