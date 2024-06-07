// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public partial class TestSceneJuiceStreamSelectionBlueprint : CatchSelectionBlueprintTestScene
    {
        private JuiceStream hitObject;

        private readonly ManualClock manualClock = new ManualClock();

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            EditorBeatmap.Clear();
            Content.Clear();

            manualClock.CurrentTime = 0;
            Content.Clock = new FramedClock(manualClock);

            InputManager.ReleaseButton(MouseButton.Left);
            InputManager.ReleaseKey(Key.ShiftLeft);
            InputManager.ReleaseKey(Key.ControlLeft);
        });

        [Test]
        public void TestBasicComponentLayout()
        {
            double[] times = { 100, 300, 500 };
            float[] positions = { 100, 200, 100 };
            addBlueprintStep(times, positions);

            for (int i = 0; i < times.Length; i++)
                addVertexCheckStep(times.Length, i, times[i], positions[i]);

            AddAssert("correct outline count", () =>
            {
                int expected = hitObject.NestedHitObjects.Count(h => !(h is TinyDroplet));
                return this.ChildrenOfType<FruitOutline>().Count() == expected;
            });
            AddAssert("correct vertex piece count", () =>
                this.ChildrenOfType<VertexPiece>().Count() == times.Length);

            AddAssert("first vertex is semitransparent", () =>
                Precision.DefinitelyBigger(1, this.ChildrenOfType<VertexPiece>().First().Alpha));
        }

        [Test]
        public void TestVertexDrag()
        {
            double[] times = { 100, 400, 700 };
            float[] positions = { 100, 100, 100 };
            addBlueprintStep(times, positions);

            addDragStartStep(times[1], positions[1]);

            AddMouseMoveStep(500, 150);
            addVertexCheckStep(3, 1, 500, 150);

            addDragEndStep();
            addDragStartStep(times[2], positions[2]);

            AddMouseMoveStep(300, 50);
            addVertexCheckStep(3, 1, 300, 50);
            addVertexCheckStep(3, 2, 500, 150);

            AddMouseMoveStep(-100, 100);
            addVertexCheckStep(3, 1, times[0], positions[0]);
        }

        [Test]
        public void TestMultipleDrag()
        {
            double[] times = { 100, 300, 500, 700 };
            float[] positions = { 100, 100, 100, 100 };
            addBlueprintStep(times, positions);

            AddMouseMoveStep(times[1], positions[1]);
            AddStep("press left", () => InputManager.PressButton(MouseButton.Left));
            AddStep("release left", () => InputManager.ReleaseButton(MouseButton.Left));
            AddStep("hold control", () => InputManager.PressKey(Key.ControlLeft));
            addDragStartStep(times[2], positions[2]);

            AddMouseMoveStep(times[2] - 50, positions[2] - 50);
            addVertexCheckStep(4, 1, times[1] - 50, positions[1] - 50);
            addVertexCheckStep(4, 2, times[2] - 50, positions[2] - 50);
        }

        [Test]
        public void TestSliderVelocityChange()
        {
            double[] times = { 100, 300 };
            float[] positions = { 200, 300 };
            addBlueprintStep(times, positions);
            AddAssert("default slider velocity", () => hitObject.SliderVelocityMultiplierBindable.IsDefault);

            addDragStartStep(times[1], positions[1]);
            AddMouseMoveStep(times[1], 400);
            AddAssert("slider velocity changed", () => !hitObject.SliderVelocityMultiplierBindable.IsDefault);
        }

        [Test]
        public void TestScrollWhileDrag()
        {
            double[] times = { 300, 500 };
            float[] positions = { 100, 100 };
            addBlueprintStep(times, positions);

            addDragStartStep(times[1], positions[1]);
            // This mouse move is necessary to start drag and capture the input.
            AddMouseMoveStep(times[1], positions[1] + 50);

            AddStep("scroll playfield", () => manualClock.CurrentTime += 200);
            AddMouseMoveStep(times[1] + 200, positions[1] + 100);
            addVertexCheckStep(2, 1, times[1] + 200, positions[1] + 100);
        }

        [Test]
        public void TestUpdateFromHitObject()
        {
            double[] times = { 100, 300 };
            float[] positions = { 200, 200 };
            addBlueprintStep(times, positions);

            AddStep("update hit object path", () =>
            {
                hitObject.Path = new SliderPath(PathType.PERFECT_CURVE, new[]
                {
                    Vector2.Zero,
                    new Vector2(100, 100),
                    new Vector2(0, 200),
                });
                EditorBeatmap.Update(hitObject);
            });
            AddAssert("path is updated", () => getVertices().Count > 2);
        }

        [Test]
        public void TestAddVertex()
        {
            double[] times = { 100, 700 };
            float[] positions = { 200, 200 };
            addBlueprintStep(times, positions, 0.2);

            addAddVertexSteps(500, 150);
            addVertexCheckStep(3, 1, 500, 150);

            addAddVertexSteps(90, 200);
            addVertexCheckStep(4, 1, times[0], positions[0]);

            addAddVertexSteps(750, 180);
            addVertexCheckStep(5, 4, 750, 180);
            AddAssert("duration is changed", () => Precision.AlmostEquals(hitObject.Duration, 800 - times[0], 1e-3));
        }

        [Test]
        public void TestDeleteVertex()
        {
            double[] times = { 100, 300, 500 };
            float[] positions = { 100, 200, 150 };
            addBlueprintStep(times, positions);

            addDeleteVertexSteps(times[1], positions[1]);
            addVertexCheckStep(2, 1, times[2], positions[2]);

            // The first vertex cannot be deleted.
            addDeleteVertexSteps(times[0], positions[0]);
            addVertexCheckStep(2, 0, times[0], positions[0]);

            addDeleteVertexSteps(times[2], positions[2]);
            addVertexCheckStep(1, 0, times[0], positions[0]);
        }

        [Test]
        public void TestVertexResampling()
        {
            addBlueprintStep(100, 100, new SliderPath(PathType.PERFECT_CURVE, new[]
            {
                Vector2.Zero,
                new Vector2(100, 100),
                new Vector2(50, 200),
            }), 0.5);
            AddAssert("1 vertex per 1 nested HO", () => getVertices().Count == hitObject.NestedHitObjects.Count);
            AddAssert("slider path not yet changed", () => hitObject.Path.ControlPoints[0].Type == PathType.PERFECT_CURVE);
            addAddVertexSteps(150, 150);
            AddAssert("slider path change to linear", () => hitObject.Path.ControlPoints[0].Type == PathType.LINEAR);
        }

        private void addBlueprintStep(double time, float x, SliderPath sliderPath, double velocity) => AddStep("add selection blueprint", () =>
        {
            hitObject = new JuiceStream
            {
                StartTime = time,
                X = x,
                Path = sliderPath,
            };
            EditorBeatmap.Difficulty.SliderMultiplier = velocity;
            EditorBeatmap.Add(hitObject);
            EditorBeatmap.Update(hitObject);
            Assert.That(hitObject.Velocity, Is.EqualTo(velocity));
            AddBlueprint(new JuiceStreamSelectionBlueprint(hitObject));
        });

        private void addBlueprintStep(double[] times, float[] positions, double velocity = 0.5)
        {
            var path = new JuiceStreamPath();
            for (int i = 1; i < times.Length; i++)
                path.Add(times[i] - times[0], positions[i] - positions[0]);

            var sliderPath = new SliderPath();
            path.ConvertToSliderPath(sliderPath, 0, velocity);
            addBlueprintStep(times[0], positions[0], sliderPath, velocity);
        }

        private IReadOnlyList<JuiceStreamPathVertex> getVertices() => this.ChildrenOfType<EditablePath>().Single().Vertices;

        private void addVertexCheckStep(int count, int index, double time, float x) => AddAssert($"vertex {index} of {count} at {time}, {x}", () =>
        {
            double expectedTime = time - hitObject.StartTime;
            float expectedX = x - hitObject.OriginalX;
            var vertices = getVertices();
            return vertices.Count == count &&
                   Precision.AlmostEquals(vertices[index].Time, expectedTime, 1e-3) &&
                   Precision.AlmostEquals(vertices[index].X, expectedX);
        });

        private void addDragStartStep(double time, float x)
        {
            AddMouseMoveStep(time, x);
            AddStep("start dragging", () => InputManager.PressButton(MouseButton.Left));
        }

        private void addDragEndStep() => AddStep("end dragging", () => InputManager.ReleaseButton(MouseButton.Left));

        private void addAddVertexSteps(double time, float x)
        {
            AddMouseMoveStep(time, x);
            AddStep("add vertex", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
        }

        private void addDeleteVertexSteps(double time, float x)
        {
            AddMouseMoveStep(time, x);
            AddStep("delete vertex", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
        }
    }
}
