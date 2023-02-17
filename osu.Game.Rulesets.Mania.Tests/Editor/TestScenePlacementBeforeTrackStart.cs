// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestScenePlacementBeforeTrackStart : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new ManiaRuleset();

        [Test]
        public void TestPlacement()
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
    }
}
