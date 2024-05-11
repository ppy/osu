// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneObjectObjectSnap : TestSceneOsuEditor
    {
        private OsuPlayfield playfield;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(Ruleset.Value, false);

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("get playfield", () => playfield = Editor.ChildrenOfType<OsuPlayfield>().First());
            AddStep("seek to first control point", () => EditorClock.Seek(Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.First().Time));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestHitCircleSnapsToOtherHitCircle(bool distanceSnapEnabled)
        {
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre));

            if (!distanceSnapEnabled)
                AddStep("disable distance snap", () => InputManager.Key(Key.Q));

            AddStep("enter placement mode", () => InputManager.Key(Key.Number2));

            AddStep("place first object", () => InputManager.Click(MouseButton.Left));

            AddStep("move mouse slightly", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(playfield.ScreenSpaceDrawQuad.Width * 0.01f, 0)));

            AddStep("place second object", () => InputManager.Click(MouseButton.Left));

            AddAssert("both objects at same location", () =>
            {
                var objects = EditorBeatmap.HitObjects;

                var first = (OsuHitObject)objects.First();
                var second = (OsuHitObject)objects.Last();

                return Precision.AlmostEquals(first.EndPosition, second.Position);
            });
        }

        [Test]
        public void TestHitCircleSnapsToSliderEnd()
        {
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre));

            AddStep("disable distance snap", () => InputManager.Key(Key.Q));

            AddStep("enter slider placement mode", () => InputManager.Key(Key.Number3));

            AddStep("start slider placement", () => InputManager.Click(MouseButton.Left));

            AddStep("move to place end", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(playfield.ScreenSpaceDrawQuad.Width * 0.225f, 0)));

            AddStep("end slider placement", () => InputManager.Click(MouseButton.Right));

            AddStep("enter circle placement mode", () => InputManager.Key(Key.Number2));

            AddStep("move mouse slightly", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(playfield.ScreenSpaceDrawQuad.Width * 0.205f, 0)));

            AddStep("place second object", () => InputManager.Click(MouseButton.Left));

            AddAssert("circle is at slider's end", () =>
            {
                var objects = EditorBeatmap.HitObjects;

                var first = (Slider)objects.First();
                var second = (OsuHitObject)objects.Last();

                return Precision.AlmostEquals(first.EndPosition, second.Position);
            });
        }

        [Test]
        public void TestSecondCircleInSelectionAlsoSnaps()
        {
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre));

            AddStep("disable distance snap", () => InputManager.Key(Key.Q));

            AddStep("enter placement mode", () => InputManager.Key(Key.Number2));

            AddStep("place first object", () => InputManager.Click(MouseButton.Left));

            AddStep("increment time", () => EditorClock.SeekForward(true));

            AddStep("move mouse right", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(playfield.ScreenSpaceDrawQuad.Width * 0.2f, 0)));
            AddStep("place second object", () => InputManager.Click(MouseButton.Left));

            AddStep("increment time", () => EditorClock.SeekForward(true));

            AddStep("move mouse down", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(0, playfield.ScreenSpaceDrawQuad.Width * 0.2f)));
            AddStep("place third object", () => InputManager.Click(MouseButton.Left));

            AddStep("enter selection mode", () => InputManager.Key(Key.Number1));

            AddStep("select objects 2 and 3", () =>
            {
                // add selection backwards to test non-sequential time ordering
                EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects[2]);
                EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects[1]);
            });

            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));

            AddStep("move mouse slightly off centre", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(playfield.ScreenSpaceDrawQuad.Width * 0.01f, 0)));

            AddAssert("object 3 snapped to 1", () =>
            {
                var objects = EditorBeatmap.HitObjects;

                var first = (OsuHitObject)objects.First();
                var third = (OsuHitObject)objects.Last();

                return Precision.AlmostEquals(first.EndPosition, third.Position);
            });

            AddStep("move mouse slightly off centre", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(playfield.ScreenSpaceDrawQuad.Width * -0.21f, playfield.ScreenSpaceDrawQuad.Width * 0.205f)));

            AddAssert("object 2 snapped to 1", () =>
            {
                var objects = EditorBeatmap.HitObjects;

                var first = (OsuHitObject)objects.First();
                var second = (OsuHitObject)objects.ElementAt(1);

                return Precision.AlmostEquals(first.EndPosition, second.Position);
            });

            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));
        }
    }
}
