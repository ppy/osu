// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public partial class TestSceneCatchEditorSaving : EditorSavingTestScene
    {
        protected override Ruleset CreateRuleset() => new CatchRuleset();

        [Test]
        public void TestCatchJuiceStreamTickCorrect()
        {
            AddStep("enter timing mode", () => InputManager.Key(Key.F3));
            AddStep("add timing point", () => EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint()));
            AddStep("enter compose mode", () => InputManager.Key(Key.F1));

            Vector2 startPoint = Vector2.Zero;
            float increment = 0;

            AddUntilStep("wait for playfield", () => this.ChildrenOfType<CatchPlayfield>().FirstOrDefault()?.IsLoaded == true);
            AddStep("move to centre", () =>
            {
                var playfield = this.ChildrenOfType<CatchPlayfield>().Single();
                startPoint = playfield.ScreenSpaceDrawQuad.Centre + new Vector2(0, playfield.ScreenSpaceDrawQuad.Height / 3);
                increment = playfield.ScreenSpaceDrawQuad.Height / 10;
                InputManager.MoveMouseTo(startPoint);
            });
            AddStep("choose juice stream placing tool", () => InputManager.Key(Key.Number3));
            AddStep("start placement", () => InputManager.Click(MouseButton.Left));

            AddStep("move to next", () => InputManager.MoveMouseTo(startPoint + new Vector2(2 * increment, -increment)));
            AddStep("add node", () => InputManager.Click(MouseButton.Left));

            AddStep("move to next", () => InputManager.MoveMouseTo(startPoint + new Vector2(-2 * increment, -2 * increment)));
            AddStep("add node", () => InputManager.Click(MouseButton.Left));

            AddStep("move to next", () => InputManager.MoveMouseTo(startPoint + new Vector2(0, -3 * increment)));
            AddStep("end placement", () => InputManager.Click(MouseButton.Right));

            AddUntilStep("juice stream placed", () => EditorBeatmap.HitObjects, () => Has.Count.EqualTo(1));

            int largeDropletCount = 0, tinyDropletCount = 0;
            AddStep("store droplet count", () =>
            {
                largeDropletCount = EditorBeatmap.HitObjects[0].NestedHitObjects.Count(t => t.GetType() == typeof(Droplet));
                tinyDropletCount = EditorBeatmap.HitObjects[0].NestedHitObjects.Count(t => t.GetType() == typeof(TinyDroplet));
            });

            SaveEditor();
            ReloadEditorToSameBeatmap();

            AddAssert("large droplet count is the same", () => EditorBeatmap.HitObjects[0].NestedHitObjects.Count(t => t.GetType() == typeof(Droplet)), () => Is.EqualTo(largeDropletCount));
            AddAssert("tiny droplet count is the same", () => EditorBeatmap.HitObjects[0].NestedHitObjects.Count(t => t.GetType() == typeof(TinyDroplet)), () => Is.EqualTo(tinyDropletCount));
        }
    }
}
