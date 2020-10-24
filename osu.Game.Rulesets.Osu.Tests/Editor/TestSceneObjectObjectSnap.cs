// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class TestSceneObjectObjectSnap : TestSceneOsuEditor
    {
        private OsuPlayfield playfield;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(Ruleset.Value, false);

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("get playfield", () => playfield = Editor.ChildrenOfType<OsuPlayfield>().First());
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

            AddStep("move mouse slightly", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(playfield.ScreenSpaceDrawQuad.Width * 0.02f, 0)));

            AddStep("place second object", () => InputManager.Click(MouseButton.Left));

            AddAssert("both objects at same location", () =>
            {
                var objects = EditorBeatmap.HitObjects;

                var first = (OsuHitObject)objects.First();
                var second = (OsuHitObject)objects.Last();

                return first.Position == second.Position;
            });
        }

        [Test]
        public void TestHitCircleSnapsToSliderEnd()
        {
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre));

            AddStep("disable distance snap", () => InputManager.Key(Key.Q));

            AddStep("enter slider placement mode", () => InputManager.Key(Key.Number3));

            AddStep("start slider placement", () => InputManager.Click(MouseButton.Left));

            AddStep("move to place end", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(playfield.ScreenSpaceDrawQuad.Width * 0.185f, 0)));

            AddStep("end slider placement", () => InputManager.Click(MouseButton.Right));

            AddStep("enter circle placement mode", () => InputManager.Key(Key.Number2));

            AddStep("move mouse slightly", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(playfield.ScreenSpaceDrawQuad.Width * 0.20f, 0)));

            AddStep("place second object", () => InputManager.Click(MouseButton.Left));

            AddAssert("circle is at slider's end", () =>
            {
                var objects = EditorBeatmap.HitObjects;

                var first = (Slider)objects.First();
                var second = (OsuHitObject)objects.Last();

                return Precision.AlmostEquals(first.EndPosition, second.Position);
            });
        }
    }
}
