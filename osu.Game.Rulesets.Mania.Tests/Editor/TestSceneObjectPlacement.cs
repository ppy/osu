// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestSceneObjectPlacement : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new ManiaRuleset();

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Test]
        public void TestPlacementBeforeTrackStart()
        {
            AddStep("Seek to 0", () => EditorClock.Seek(0));
            AddStep("Select note", () => InputManager.Key(Key.Number2));
            AddStep("Hover negative span", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<Container>().First(x => x.Name == "Icons").Children[0]);
            });
            AddStep("Click", () => InputManager.Click(MouseButton.Left));
            AddAssert("No notes placed", () => EditorBeatmap.HitObjects.All(x => x.StartTime >= 0));
        }

        [Test]
        public void TestSeekOnNotePlacement()
        {
            double? initialTime = null;

            AddStep("store initial time", () => initialTime = EditorClock.CurrentTime);
            AddStep("change seek setting to true", () => config.SetValue(OsuSetting.EditorAutoSeekOnPlacement, true));
            placeObject();
            AddUntilStep("wait for seek to complete", () => !EditorClock.IsSeeking);
            AddAssert("seeked forward to object", () => EditorClock.CurrentTime, () => Is.GreaterThan(initialTime));
        }

        [Test]
        public void TestNoSeekOnNotePlacement()
        {
            double? initialTime = null;

            AddStep("store initial time", () => initialTime = EditorClock.CurrentTime);
            AddStep("change seek setting to false", () => config.SetValue(OsuSetting.EditorAutoSeekOnPlacement, false));
            placeObject();
            AddAssert("not seeking", () => !EditorClock.IsSeeking);
            AddAssert("time is unchanged", () => EditorClock.CurrentTime, () => Is.EqualTo(initialTime));
        }

        private void placeObject()
        {
            AddStep("select note placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to centre of last column", () => InputManager.MoveMouseTo(this.ChildrenOfType<Column>().Last().ScreenSpaceDrawQuad.Centre));
            AddStep("place note", () => InputManager.Click(MouseButton.Left));
        }
    }
}
