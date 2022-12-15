// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestScenePreviewTime : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestSetPreviewTimingPoint()
        {
            AddStep("seek to 1000", () => EditorClock.Seek(1000));
            AddAssert("time is 1000", () => EditorClock.CurrentTime == 1000);
            AddStep("set current time as preview point", () => Editor.SetCurrectTimeAsPreview());
            AddAssert("preview time is 1000", () => EditorBeatmap.PreviewTime.Value == 1000);
        }
    }
}
