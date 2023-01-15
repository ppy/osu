// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestScenePreviewTime : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestSceneSetPreviewTimingPoint()
        {
            AddStep("seek to 1000", () => EditorClock.Seek(1000));
            AddAssert("time is 1000", () => EditorClock.CurrentTime == 1000);
            AddStep("set current time as preview point", () => Editor.SetPreviewPointToCurrentTime());
            AddAssert("preview time is 1000", () => EditorBeatmap.PreviewTime.Value == 1000);
        }

        [Test]
        public void TestScenePreviewTimeline()
        {
            AddStep("set preview time to -1", () => EditorBeatmap.PreviewTime.Value = -1);
            AddAssert("preview time line should not show", () => !Editor.ChildrenOfType<PreviewTimePart>().Single().Children.Any());
            AddStep("set preview time to 1000", () => EditorBeatmap.PreviewTime.Value = 1000);
            AddAssert("preview time line should show", () => Editor.ChildrenOfType<PreviewTimePart>().Single().Children.Single().Alpha == 1);
        }
    }
}
