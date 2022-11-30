// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneStreamCreation : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(Ruleset.Value, false);

        private ComposeBlueprintContainer blueprintContainer
            => Editor.ChildrenOfType<ComposeBlueprintContainer>().First();

        private double firstTimingPointTime() => Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.First().Time;

        private OsuPlayfield playfield = null!;
        private Stream? stream;
        private PathControlPointVisualiser<Stream>? visualiser;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("get playfield", () => playfield = Editor.ChildrenOfType<OsuPlayfield>().First());
            AddStep("seek to first timing point", () => EditorClock.Seek(firstTimingPointTime()));
            AddStep("select stream creation tool", () => InputManager.Key(Key.Number5));
            addMovementStep(new Vector2(10));
            addClickStep(MouseButton.Left);
            assertPlaced(true);
            AddStep("get stream", () =>
            {
                stream = getStream();
                visualiser = blueprintContainer.ChildrenOfType<PathControlPointVisualiser<Stream>>().First();
            });
        }

        [Test]
        public void TestCreateSimpleStream()
        {
            addMovementStep(new Vector2(210, 10));
            addSeekStep(4);
            addClickStep(MouseButton.Right);
            assertCircleCount(5);
            assertControlPointTime(1, 4);
            addMovementStep(new Vector2(210));
            addClickStep(MouseButton.Left);
            assertPlaced(false);
            assertHitCircles(Enumerable.Range(0, 5).Select(i => (new Vector2(10 + 50 * i, 10), i)).ToArray());
        }

        [Test]
        public void TestCreateComplexStream()
        {
            addMovementStep(new Vector2(210, 10));
            addSeekStep(4);
            addClickStep(MouseButton.Left);
            addClickStep(MouseButton.Left);
            addMovementStep(new Vector2(310, 110));
            addClickStep(MouseButton.Left);
            addMovementStep(new Vector2(210, 210));
            addSeekStep(8);
            addAccelStep(2f);
            addClickStep(MouseButton.Left);
            addClickStep(MouseButton.Left);
            addMovementStep(new Vector2(310, 260));
            addClickStep(MouseButton.Left);
            addMovementStep(new Vector2(310, 160));
            addClickStep(MouseButton.Left);
            addMovementStep(new Vector2(410, 210));
            addSnapStep(8);
            addSeekStep(24);
            addClickStep(MouseButton.Right);
            assertCircleCount(17);
            assertControlPointTime(0, 0);
            assertControlPointTime(1, 8);
            assertControlPointTime(2, 16);
            assertControlPointTime(3, 24);
            assertControlPointAccel(2, 2);
            assertNewCombo(0);
            AddStep("toggle new combo", () => InputManager.Key(Key.Q));
            assertNewCombo(0, 4, 8);
            addMovementStep(new Vector2(512, 386));
            AddStep("start drag select", () => InputManager.PressButton(MouseButton.Left));
            AddStep("drag 50x50", () => InputManager.MoveMouseTo(playfield.ToScreenSpace(new Vector2(462, 336))));
            AddStep("end drag select", () => InputManager.ReleaseButton(MouseButton.Left));
        }

        private void addMovementStep(Vector2 position) => AddStep($"move mouse to {position}", () => InputManager.MoveMouseTo(playfield.ToScreenSpace(position)));

        private void addClickStep(MouseButton button) => AddStep($"click {button}", () => InputManager.Click(button));

        private void addSeekStep(int ticks) => AddStep($"seek to {ticks} beat lengths", () => EditorClock.Seek(getTime(ticks)));

        private void addAccelStep(float amount) => AddStep($"change acceleration by {amount}", () =>
        {
            InputManager.PressKey(Key.ShiftLeft);
            InputManager.ScrollHorizontalBy(amount * 2);
            InputManager.ReleaseKey(Key.ShiftLeft);
        });

        private void addSnapStep(int snap) => AddStep($"change snap to {snap}", () =>
        {
            InputManager.PressKey(Key.ShiftLeft);
            InputManager.Key(Key.Number0 + snap);
            InputManager.ReleaseKey(Key.ShiftLeft);
        });

        private void assertPlaced(bool expected) => AddAssert($"stream {(expected ? "placed" : "not placed")}", () => getStream() != null == expected);

        private void assertCircleCount(int expected) => AddAssert($"has {expected} circles", () => stream != null && stream.NestedHitObjects.Count == expected && stream.HitCircleStates.Count == expected && stream.StreamPath.GetStreamPath().Count == expected);

        private void assertControlPointTime(int index, int ticks) =>
            AddAssert($"stream control point {index} at tick {ticks}", () => stream != null && Precision.AlmostEquals(getTime(ticks), stream.StartTime + stream.StreamPath.ControlPoints[index].Time, 1));

        private void assertControlPointAccel(int index, double amount) =>
            AddAssert($"stream control point {index} accel {amount}", () => stream != null && Precision.AlmostEquals(amount, stream.StreamPath.ControlPoints[index].Acceleration, 1E-3));

        private Stream? getStream() => EditorBeatmap.PlacementObject.Value as Stream ?? EditorBeatmap.HitObjects.OfType<Stream>().FirstOrDefault();

        private double getTime(int ticks) => firstTimingPointTime() + EditorBeatmap.GetBeatLengthAtTime(firstTimingPointTime()) * ticks;

        private void assertHitCircles(params (Vector2, int)[] points)
        {
            AddAssert("hit circles created", () =>
            {
                var circles = EditorBeatmap.HitObjects.OfType<HitCircle>().ToArray();

                if (circles.Length != points.Length) return false;

                for (int i = 0; i < circles.Length; i++)
                {
                    var (p, t) = points[i];
                    double time = getTime(t);
                    if (!Precision.AlmostEquals(p, circles[i].Position, 1) || !Precision.AlmostEquals(time, circles[i].StartTime, 1))
                        return false;
                }

                return true;
            });
        }

        private void assertNewCombo(params int[] indices)
        {
            AddAssert($"new combo", () =>
            {
                var circles = EditorBeatmap.HitObjects.OfType<HitCircle>().ToArray();

                for (int i = 0; i < circles.Length; i++)
                {
                    if (indices.Contains(i))
                    {
                        if (!circles[i].NewCombo)
                            return false;
                    }
                    else
                    {
                        if (circles[i].NewCombo)
                            return false;
                    }
                }

                return true;
            });
        }
    }
}
