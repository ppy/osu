// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
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

        private void addMovementStep(Vector2 position) => AddStep($"move mouse to {position}", () => InputManager.MoveMouseTo(playfield.ToScreenSpace(position)));

        private void addClickStep(MouseButton button) => AddStep($"click {button}", () => InputManager.Click(button));

        private void addSeekStep(double amount) => AddStep($"seek {amount} beat lengths", () => EditorClock.SeekForward(true, amount));

        private void assertPlaced(bool expected) => AddAssert($"stream {(expected ? "placed" : "not placed")}", () => getStream() != null == expected);

        private void assertCircleCount(int expected) => AddAssert($"has {expected} circles", () => stream != null && stream.NestedHitObjects.Count == expected && stream.HitCircleStates.Count == expected && stream.StreamPath.GetStreamPath().Count == expected);

        private void assertControlPointTime(int index, int ticks) =>
            AddAssert($"stream control point {index} at tick {ticks}", () => stream != null && Precision.AlmostEquals(getTime(ticks), stream.StartTime + stream.StreamPath.ControlPoints[index].Time, 1));

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
    }
}
