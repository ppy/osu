// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestScenePathControlPointVisualiser : OsuManualInputManagerTestScene
    {
        private Slider slider;
        private PathControlPointVisualiser<Slider> visualiser;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            slider = new Slider();
            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
        });

        [Test]
        public void TestPerfectCurveTooManyPoints()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.BEZIER);
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200));
            addControlPointStep(new Vector2(500, 100));

            // Must be both hovering and selecting the control point for the context menu to work.
            moveMouseToControlPoint(1);
            AddStep("select control point", () => visualiser.Pieces[1].IsSelected.Value = true);
            addContextMenuItemStep("Perfect curve");

            assertControlPointPathType(0, PathType.BEZIER);
            assertControlPointPathType(1, PathType.PERFECT_CURVE);
            assertControlPointPathType(3, PathType.BEZIER);
        }

        [Test]
        public void TestPerfectCurveLastThreePoints()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.BEZIER);
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200));
            addControlPointStep(new Vector2(500, 100));

            moveMouseToControlPoint(2);
            AddStep("select control point", () => visualiser.Pieces[2].IsSelected.Value = true);
            addContextMenuItemStep("Perfect curve");

            assertControlPointPathType(0, PathType.BEZIER);
            assertControlPointPathType(2, PathType.PERFECT_CURVE);
            assertControlPointPathType(4, null);
        }

        [Test]
        public void TestPerfectCurveLastTwoPoints()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.BEZIER);
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200));
            addControlPointStep(new Vector2(500, 100));

            moveMouseToControlPoint(3);
            AddStep("select control point", () => visualiser.Pieces[3].IsSelected.Value = true);
            addContextMenuItemStep("Perfect curve");

            assertControlPointPathType(0, PathType.BEZIER);
            AddAssert("point 3 is not inherited", () => slider.Path.ControlPoints[3].Type != null);
        }

        [Test]
        public void TestPerfectCurveTooManyPointsLinear()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.LINEAR);
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200));
            addControlPointStep(new Vector2(500, 100));

            // Must be both hovering and selecting the control point for the context menu to work.
            moveMouseToControlPoint(1);
            AddStep("select control point", () => visualiser.Pieces[1].IsSelected.Value = true);
            addContextMenuItemStep("Perfect curve");

            assertControlPointPathType(0, PathType.LINEAR);
            assertControlPointPathType(1, PathType.PERFECT_CURVE);
            assertControlPointPathType(3, PathType.LINEAR);
        }

        [Test]
        public void TestPerfectCurveChangeToBezier()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.BEZIER);
            addControlPointStep(new Vector2(300), PathType.PERFECT_CURVE);
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200), PathType.BEZIER);
            addControlPointStep(new Vector2(500, 100));

            moveMouseToControlPoint(3);
            AddStep("select control point", () => visualiser.Pieces[3].IsSelected.Value = true);
            addContextMenuItemStep("Inherit");

            assertControlPointPathType(0, PathType.BEZIER);
            assertControlPointPathType(1, PathType.BEZIER);
            assertControlPointPathType(3, null);
        }

        [Test]
        public void TestCatmullAvailableIffSelectionContainsCatmull()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.CATMULL);
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200));
            addControlPointStep(new Vector2(500, 100));

            moveMouseToControlPoint(2);
            AddStep("select first and third control point", () =>
            {
                visualiser.Pieces[0].IsSelected.Value = true;
                visualiser.Pieces[2].IsSelected.Value = true;
            });
            addContextMenuItemStep("Catmull");

            assertControlPointPathType(0, PathType.CATMULL);
            assertControlPointPathType(2, PathType.CATMULL);
            assertControlPointPathType(4, null);
        }

        [Test]
        public void TestStackingUpdatesPointsPosition()
        {
            createVisualiser(true);

            Vector2[] points =
            [
                new Vector2(200),
                new Vector2(300),
                new Vector2(500, 300),
                new Vector2(700, 200),
                new Vector2(500, 100)
            ];

            foreach (var point in points) addControlPointStep(point);

            AddStep("apply stacking", () => slider.StackHeightBindable.Value += 1);

            for (int i = 0; i < points.Length; i++)
                addAssertPointPositionChanged(points, i);
        }

        private void addAssertPointPositionChanged(Vector2[] points, int index)
        {
            AddAssert($"Point at {points.ElementAt(index)} changed",
                () => visualiser.Pieces[index].Position,
                () => !Is.EqualTo(points.ElementAt(index))
            );
        }

        private void createVisualiser(bool allowSelection) => AddStep("create visualiser", () => Child = visualiser = new PathControlPointVisualiser<Slider>(slider, allowSelection)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        });

        private void addControlPointStep(Vector2 position) => addControlPointStep(position, null);

        private void addControlPointStep(Vector2 position, PathType? type)
        {
            AddStep($"add {type?.Type} control point at {position}", () =>
            {
                slider.Path.ControlPoints.Add(new PathControlPoint(position, type));
            });
        }

        private void moveMouseToControlPoint(int index)
        {
            AddStep($"move mouse to control point {index}", () =>
            {
                Vector2 position = slider.Path.ControlPoints[index].Position;
                InputManager.MoveMouseTo(visualiser.Pieces[0].Parent!.ToScreenSpace(position));
            });
        }

        private void assertControlPointPathType(int controlPointIndex, PathType? type)
        {
            AddAssert($"point {controlPointIndex} is {type}", () => slider.Path.ControlPoints[controlPointIndex].Type == type);
        }

        private void addContextMenuItemStep(string contextMenuText)
        {
            AddStep($"click context menu item \"{contextMenuText}\"", () =>
            {
                MenuItem item = visualiser.ContextMenuItems!.FirstOrDefault(menuItem => menuItem.Text.Value == "Curve type")?.Items.FirstOrDefault(menuItem => menuItem.Text.Value == contextMenuText);

                item?.Action.Value?.Invoke();
            });
        }
    }
}
