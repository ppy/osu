// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Rulesets.Osu.Objects;
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
            AddStep("enable distance snap grid", () => InputManager.Key(Key.Y));
            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));

            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            gridActive<RectangularPositionSnapGrid>(false);

            AddStep("enable rectangular grid", () => InputManager.Key(Key.T));

            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));
            AddUntilStep("distance snap grid still visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            gridActive<RectangularPositionSnapGrid>(true);

            AddStep("disable distance snap grid", () => InputManager.Key(Key.Y));
            AddUntilStep("distance snap grid hidden", () => !this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));
            gridActive<RectangularPositionSnapGrid>(true);

            AddStep("disable rectangular grid", () => InputManager.Key(Key.T));
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

            AddStep("enable distance snap grid", () => InputManager.Key(Key.Y));
            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddStep("hold alt", () => InputManager.PressKey(Key.AltLeft));
            AddUntilStep("distance snap grid hidden", () => !this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddStep("release alt", () => InputManager.ReleaseKey(Key.AltLeft));
            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
        }

        [Test]
        public void TestDistanceSnapAdjustDoesNotHideTheGridIfStartingEnabled()
        {
            double distanceSnap = double.PositiveInfinity;

            AddStep("enable distance snap grid", () => InputManager.Key(Key.Y));

            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));
            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddStep("store distance snap", () => distanceSnap = this.ChildrenOfType<IDistanceSnapProvider>().First().DistanceSpacingMultiplier.Value);

            AddStep("increase distance", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.PressKey(Key.ControlLeft);
                InputManager.ScrollVerticalBy(1);
                InputManager.ReleaseKey(Key.ControlLeft);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            AddUntilStep("distance snap increased", () => this.ChildrenOfType<IDistanceSnapProvider>().First().DistanceSpacingMultiplier.Value, () => Is.GreaterThan(distanceSnap));
            AddUntilStep("distance snap grid still visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
        }

        [Test]
        public void TestDistanceSnapAdjustShowsGridMomentarilyIfStartingDisabled()
        {
            double distanceSnap = double.PositiveInfinity;

            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));
            AddUntilStep("distance snap grid hidden", () => !this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddStep("store distance snap", () => distanceSnap = this.ChildrenOfType<IDistanceSnapProvider>().First().DistanceSpacingMultiplier.Value);

            AddStep("start increasing distance", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.PressKey(Key.ControlLeft);
            });

            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());

            AddStep("finish increasing distance", () =>
            {
                InputManager.ScrollVerticalBy(1);
                InputManager.ReleaseKey(Key.ControlLeft);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            AddUntilStep("distance snap increased", () => this.ChildrenOfType<IDistanceSnapProvider>().First().DistanceSpacingMultiplier.Value, () => Is.GreaterThan(distanceSnap));
            AddUntilStep("distance snap hidden in the end", () => !this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
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
            AddAssert($"grid type is {typeof(T).Name}", () => this.ChildrenOfType<T>().Any());
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
                TriangularPositionSnapGrid triangular => triangular.StartPosition.Value + GeometryUtils.RotateVector(
                    new Vector2(triangular.Spacing.Value / 2, triangular.Spacing.Value / 2 * MathF.Sqrt(3)), -triangular.GridLineRotation.Value),
                CircularPositionSnapGrid circular => circular.StartPosition.Value + GeometryUtils.RotateVector(new Vector2(circular.Spacing.Value, 0), -45),
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
                                                       && EditorBeatmap.GridSize == size);

        [Test]
        public void TestGridTypeToggling()
        {
            AddStep("enable rectangular grid", () => InputManager.Key(Key.T));
            AddUntilStep("rectangular grid visible", () => this.ChildrenOfType<RectangularPositionSnapGrid>().Any());
            gridActive<RectangularPositionSnapGrid>(true);

            nextGridTypeIs<TriangularPositionSnapGrid>();
            nextGridTypeIs<CircularPositionSnapGrid>();
            nextGridTypeIs<RectangularPositionSnapGrid>();
        }

        private void nextGridTypeIs<T>() where T : PositionSnapGrid
        {
            AddStep("toggle to next grid type", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
            gridActive<T>(true);
        }

        [Test]
        public void TestGridPlacementTool()
        {
            AddStep("enable rectangular grid", () => InputManager.Key(Key.T));

            AddStep("start grid placement", () => InputManager.Key(Key.Number5));
            AddStep("move cursor to slider head + (1, 1)", () =>
            {
                var composer = Editor.ChildrenOfType<RectangularPositionSnapGrid>().Single();
                InputManager.MoveMouseTo(composer.ToScreenSpace(((Slider)EditorBeatmap.HitObjects.First()).Position + new Vector2(1, 1)));
            });
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddStep("move cursor to slider tail + (1, 1)", () =>
            {
                var composer = Editor.ChildrenOfType<RectangularPositionSnapGrid>().Single();
                InputManager.MoveMouseTo(composer.ToScreenSpace(((Slider)EditorBeatmap.HitObjects.First()).EndPosition + new Vector2(1, 1)));
            });
            AddStep("left click", () => InputManager.Click(MouseButton.Left));

            gridActive<RectangularPositionSnapGrid>(true);
            AddAssert("grid position at slider head", () =>
            {
                var composer = Editor.ChildrenOfType<RectangularPositionSnapGrid>().Single();
                return Precision.AlmostEquals(((Slider)EditorBeatmap.HitObjects.First()).Position, composer.StartPosition.Value);
            });
            AddAssert("grid spacing is distance to slider tail", () =>
            {
                var composer = Editor.ChildrenOfType<RectangularPositionSnapGrid>().Single();
                return Precision.AlmostEquals(composer.Spacing.Value.X, 32.05, 0.01)
                       && Precision.AlmostEquals(composer.Spacing.Value.X, composer.Spacing.Value.Y);
            });
            AddAssert("grid rotation points to slider tail", () =>
            {
                var composer = Editor.ChildrenOfType<RectangularPositionSnapGrid>().Single();
                return Precision.AlmostEquals(composer.GridLineRotation.Value, 0.09, 0.01);
            });

            AddStep("start grid placement", () => InputManager.Key(Key.Number5));
            AddStep("move cursor to slider tail + (1, 1)", () =>
            {
                var composer = Editor.ChildrenOfType<RectangularPositionSnapGrid>().Single();
                InputManager.MoveMouseTo(composer.ToScreenSpace(((Slider)EditorBeatmap.HitObjects.First()).EndPosition + new Vector2(1, 1)));
            });
            AddStep("double click", () =>
            {
                InputManager.Click(MouseButton.Left);
                InputManager.Click(MouseButton.Left);
            });
            AddStep("move cursor to (0, 0)", () =>
            {
                var composer = Editor.ChildrenOfType<RectangularPositionSnapGrid>().Single();
                InputManager.MoveMouseTo(composer.ToScreenSpace(Vector2.Zero));
            });

            gridActive<RectangularPositionSnapGrid>(true);
            AddAssert("grid position at slider tail", () =>
            {
                var composer = Editor.ChildrenOfType<RectangularPositionSnapGrid>().Single();
                return Precision.AlmostEquals(((Slider)EditorBeatmap.HitObjects.First()).EndPosition, composer.StartPosition.Value);
            });
            AddAssert("grid spacing and rotation unchanged", () =>
            {
                var composer = Editor.ChildrenOfType<RectangularPositionSnapGrid>().Single();
                return Precision.AlmostEquals(composer.Spacing.Value.X, 32.05, 0.01)
                       && Precision.AlmostEquals(composer.Spacing.Value.X, composer.Spacing.Value.Y)
                       && Precision.AlmostEquals(composer.GridLineRotation.Value, 0.09, 0.01);
            });
        }
    }
}
