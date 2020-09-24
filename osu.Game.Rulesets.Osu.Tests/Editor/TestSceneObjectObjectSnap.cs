// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
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

        [Test]
        public void TestHitCircleSnapsToOtherHitCircle()
        {
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre));

            AddStep("disable distance snap", () =>
            {
                InputManager.PressKey(Key.Q);
                InputManager.ReleaseKey(Key.Q);
            });

            AddStep("enter placement mode", () =>
            {
                InputManager.PressKey(Key.Number2);
                InputManager.ReleaseKey(Key.Number2);
            });

            AddStep("place first object", () => InputManager.Click(MouseButton.Left));

            AddStep("move mouse slightly", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre + new Vector2(5)));

            AddStep("place second object", () => InputManager.Click(MouseButton.Left));

            AddAssert("both objects at same location", () =>
            {
                var objects = EditorBeatmap.HitObjects;

                var first = (OsuHitObject)objects.First();
                var second = (OsuHitObject)objects.Last();

                return first.Position == second.Position;
            });

            // TODO: remove
            AddWaitStep("wait", 10);
        }
    }
}
