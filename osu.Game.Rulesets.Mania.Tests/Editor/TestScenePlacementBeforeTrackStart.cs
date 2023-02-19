// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestScenePlacementBeforeTrackStart : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new ManiaRuleset();

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

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

        [Test]
        public void TestSeekOnNotePlacement()
        {
            AddStep("Seek to 1935", () => EditorClock.Seek(1935));
            AddStep("Change seek setting to true", () => config.SetValue(OsuSetting.EditorSeekToHitObject, true));
            seekSetup();
            AddUntilStep("Wait for seeking to end", () => !EditorClock.IsSeeking);
            AddAssert("Seeked to object", () => EditorClock.CurrentTimeAccurate == 2287.1875);
        }

        [Test]
        public void TestNoSeekOnNotePlacement()
        {
            AddStep("Seek to 1935", () => EditorClock.Seek(1935));
            AddStep("Change seek setting to false", () => config.SetValue(OsuSetting.EditorSeekToHitObject, false));
            seekSetup();
            AddAssert("Not seeking", () => !EditorClock.IsSeeking);
            AddAssert("Not seeked to object", () => EditorClock.CurrentTime == 1935);
        }

        private void seekSetup()
        {
            AddStep("Seek to 1935", () => EditorClock.Seek(1935));
            AddStep("Select note", () => InputManager.Key(Key.Number2));
            AddStep("Place note", () => InputManager.MoveMouseTo(this.ChildrenOfType<DrawableHoldNoteHead>().First(x => x.HitObject.StartTime == 2170)));
            AddStep("Click", () => InputManager.Click(MouseButton.Left));
        }
    }
}
