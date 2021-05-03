// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class TestScenePathControlPointVisualiser : OsuManualInputManagerTestScene
    {
        private Slider slider;
        private PathControlPointVisualiser visualiser;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            slider = new Slider();
            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
        });

        [Test]
        public void TestAddOverlappingControlPoints()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200));
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));

            AddAssert("last connection displayed", () =>
            {
                var lastConnection = visualiser.Connections.Last(c => c.ControlPoint.Position.Value == new Vector2(300));
                return lastConnection.DrawWidth > 50;
            });
        }

        [Test]
        public void TestPerfectCurveTooManyPoints()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.Bezier);
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200));
            addControlPointStep(new Vector2(500, 100));

            // Must be both hovering and selecting the control point for the context menu to work.
            moveMouseToControlPoint(1);
            AddStep("select control point", () => visualiser.Pieces[1].IsSelected.Value = true);
            addContextMenuItemStep("Perfect curve");

            assertControlPointPathType(0, PathType.Bezier);
            assertControlPointPathType(1, PathType.PerfectCurve);
            assertControlPointPathType(3, PathType.Bezier);
        }

        [Test]
        public void TestPerfectCurveLastThreePoints()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.Bezier);
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200));
            addControlPointStep(new Vector2(500, 100));

            moveMouseToControlPoint(2);
            AddStep("select control point", () => visualiser.Pieces[2].IsSelected.Value = true);
            addContextMenuItemStep("Perfect curve");

            assertControlPointPathType(0, PathType.Bezier);
            assertControlPointPathType(2, PathType.PerfectCurve);
            assertControlPointPathType(4, null);
        }

        [Test]
        public void TestPerfectCurveLastTwoPoints()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.Bezier);
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200));
            addControlPointStep(new Vector2(500, 100));

            moveMouseToControlPoint(3);
            AddStep("select control point", () => visualiser.Pieces[3].IsSelected.Value = true);
            addContextMenuItemStep("Perfect curve");

            assertControlPointPathType(0, PathType.Bezier);
            AddAssert("point 3 is not inherited", () => slider.Path.ControlPoints[3].Type != null);
        }

        [Test]
        public void TestPerfectCurveTooManyPointsLinear()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.Linear);
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200));
            addControlPointStep(new Vector2(500, 100));

            // Must be both hovering and selecting the control point for the context menu to work.
            moveMouseToControlPoint(1);
            AddStep("select control point", () => visualiser.Pieces[1].IsSelected.Value = true);
            addContextMenuItemStep("Perfect curve");

            assertControlPointPathType(0, PathType.Linear);
            assertControlPointPathType(1, PathType.PerfectCurve);
            assertControlPointPathType(3, PathType.Linear);
        }

        [Test]
        public void TestPerfectCurveChangeToBezier()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200), PathType.Bezier);
            addControlPointStep(new Vector2(300), PathType.PerfectCurve);
            addControlPointStep(new Vector2(500, 300));
            addControlPointStep(new Vector2(700, 200), PathType.Bezier);
            addControlPointStep(new Vector2(500, 100));

            moveMouseToControlPoint(3);
            AddStep("select control point", () => visualiser.Pieces[3].IsSelected.Value = true);
            addContextMenuItemStep("Inherit");

            assertControlPointPathType(0, PathType.Bezier);
            assertControlPointPathType(1, PathType.Bezier);
            assertControlPointPathType(3, null);
        }

        private void createVisualiser(bool allowSelection) => AddStep("create visualiser", () => Child = visualiser = new PathControlPointVisualiser(slider, allowSelection)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        });

        private void addControlPointStep(Vector2 position) => addControlPointStep(position, null);

        private void addControlPointStep(Vector2 position, PathType? type)
        {
            AddStep($"add {type} control point at {position}", () =>
            {
                slider.Path.ControlPoints.Add(new PathControlPoint(position, type));
            });
        }

        private void moveMouseToControlPoint(int index)
        {
            AddStep($"move mouse to control point {index}", () =>
            {
                Vector2 position = slider.Path.ControlPoints[index].Position.Value;
                InputManager.MoveMouseTo(visualiser.Pieces[0].Parent.ToScreenSpace(position));
            });
        }

        private void assertControlPointPathType(int controlPointIndex, PathType? type)
        {
            AddAssert($"point {controlPointIndex} is {type}", () => slider.Path.ControlPoints[controlPointIndex].Type.Value == type);
        }

        private void addContextMenuItemStep(string contextMenuText)
        {
            AddStep($"click context menu item \"{contextMenuText}\"", () =>
            {
                MenuItem item = visualiser.ContextMenuItems[1].Items.FirstOrDefault(menuItem => menuItem.Text.Value == contextMenuText);

                item?.Action?.Value();
            });
        }
    }
}
