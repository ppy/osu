// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneSliderLengthValidity : TestSceneOsuEditor
    {
        private OsuPlayfield playfield;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(Ruleset.Value, false);

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("get playfield", () => playfield = Editor.ChildrenOfType<OsuPlayfield>().First());
            AddStep("seek to first timing point", () => EditorClock.Seek(Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.First().Time));
        }

        [Test]
        public void TestDraggingStartingPointRemainsValid()
        {
            Slider slider = null;

            AddStep("Add slider", () =>
            {
                slider = new Slider { StartTime = EditorClock.CurrentTime, Position = new Vector2(300) };

                PathControlPoint[] points =
                {
                    new PathControlPoint(new Vector2(0), PathType.LINEAR),
                    new PathControlPoint(new Vector2(100, 0)),
                };

                slider.Path = new SliderPath(points);
                EditorBeatmap.Add(slider);
            });

            AddAssert("ensure object placed", () => EditorBeatmap.HitObjects.Count == 1);

            moveMouse(new Vector2(300));
            AddStep("select slider", () => InputManager.Click(MouseButton.Left));

            double distanceBefore = 0;

            AddStep("store distance", () => distanceBefore = slider.Path.Distance);

            moveMouse(new Vector2(300, 300));

            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));
            moveMouse(new Vector2(350, 300));
            moveMouse(new Vector2(400, 300));
            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("slider length shrunk", () => slider.Path.Distance < distanceBefore);
            AddAssert("ensure slider still has valid length", () => slider.Path.Distance > 0);
        }

        [Test]
        public void TestDraggingEndingPointRemainsValid()
        {
            Slider slider = null;

            AddStep("Add slider", () =>
            {
                slider = new Slider { StartTime = EditorClock.CurrentTime, Position = new Vector2(300) };

                PathControlPoint[] points =
                {
                    new PathControlPoint(new Vector2(0), PathType.LINEAR),
                    new PathControlPoint(new Vector2(100, 0)),
                };

                slider.Path = new SliderPath(points);
                EditorBeatmap.Add(slider);
            });

            AddAssert("ensure object placed", () => EditorBeatmap.HitObjects.Count == 1);

            moveMouse(new Vector2(300));
            AddStep("select slider", () => InputManager.Click(MouseButton.Left));

            double distanceBefore = 0;

            AddStep("store distance", () => distanceBefore = slider.Path.Distance);

            moveMouse(new Vector2(400, 300));

            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));
            moveMouse(new Vector2(350, 300));
            moveMouse(new Vector2(300, 300));
            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("slider length shrunk", () => slider.Path.Distance < distanceBefore);
            AddAssert("ensure slider still has valid length", () => slider.Path.Distance > 0);
        }

        /// <summary>
        /// If a control point is deleted which results in the slider becoming so short it can't exist,
        /// for simplicity delete the slider rather than having it in an invalid state.
        ///
        /// Eventually we may need to change this, based on user feedback. I think it's likely enough of
        /// an edge case that we won't get many complaints, though (and there's always the undo button).
        /// </summary>
        [Test]
        public void TestDeletingPointCausesSliderDeletion()
        {
            AddStep("Add slider", () =>
            {
                Slider slider = new Slider { StartTime = EditorClock.CurrentTime, Position = new Vector2(300) };

                PathControlPoint[] points =
                {
                    new PathControlPoint(new Vector2(0), PathType.PERFECT_CURVE),
                    new PathControlPoint(new Vector2(100, 0)),
                    new PathControlPoint(new Vector2(0, 10))
                };

                slider.Path = new SliderPath(points);
                EditorBeatmap.Add(slider);
            });

            AddAssert("ensure object placed", () => EditorBeatmap.HitObjects.Count == 1);

            AddStep("select slider", () => InputManager.Click(MouseButton.Left));

            moveMouse(new Vector2(400, 300));
            AddStep("delete second point", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Click(MouseButton.Right);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            AddAssert("ensure object deleted", () => EditorBeatmap.HitObjects.Count == 0);
        }

        /// <summary>
        /// If a scale operation is performed where a single slider is the only thing selected, the path's shape will change.
        /// If the scale results in the path becoming too short, further mouse movement in the same direction will not change the shape.
        /// </summary>
        [Test]
        public void TestScalingSliderTooSmallRemainsValid()
        {
            Slider slider = null;

            AddStep("Add slider", () =>
            {
                slider = new Slider { StartTime = EditorClock.CurrentTime, Position = new Vector2(300, 200) };

                PathControlPoint[] points =
                {
                    new PathControlPoint(new Vector2(0), PathType.LINEAR),
                    new PathControlPoint(new Vector2(0, 50)),
                    new PathControlPoint(new Vector2(0, 100))
                };

                slider.Path = new SliderPath(points);
                EditorBeatmap.Add(slider);
            });

            AddAssert("ensure object placed", () => EditorBeatmap.HitObjects.Count == 1);

            moveMouse(new Vector2(300));
            AddStep("select slider", () => InputManager.Click(MouseButton.Left));

            double distanceBefore = 0;

            AddStep("store distance", () => distanceBefore = slider.Path.Distance);

            AddStep("move mouse to handle", () => InputManager.MoveMouseTo(Editor.ChildrenOfType<SelectionBoxDragHandle>().Skip(1).First()));
            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));
            moveMouse(new Vector2(300, 300));
            moveMouse(new Vector2(300, 250));
            moveMouse(new Vector2(300, 200));
            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("slider length shrunk", () => slider.Path.Distance < distanceBefore);
            AddAssert("ensure slider still has valid length", () => slider.Path.Distance > 0);
        }

        private void moveMouse(Vector2 pos) =>
            AddStep($"move mouse to {pos}", () => InputManager.MoveMouseTo(playfield.ToScreenSpace(pos)));
    }
}
