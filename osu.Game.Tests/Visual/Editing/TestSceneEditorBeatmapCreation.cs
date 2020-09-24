// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Storyboards;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorBeatmapCreation : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null) => new DummyWorkingBeatmap(Audio, null);

        [Test]
        public void TestCreateNewBeatmap()
        {
            AddStep("save beatmap", () => Editor.Save());
            AddAssert("new beatmap persisted", () => EditorBeatmap.BeatmapInfo.ID > 0);
        }
    }
}
