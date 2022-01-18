// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Taiko;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public class TestSceneSetupScreen : EditorClockTestScene
    {
        [Cached(typeof(EditorBeatmap))]
        [Cached(typeof(IBeatSnapProvider))]
        private readonly EditorBeatmap editorBeatmap;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneSetupScreen()
        {
            editorBeatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo
                }
            });
        }

        [Test]
        public void TestOsu() => runForRuleset(new OsuRuleset().RulesetInfo);

        [Test]
        public void TestTaiko() => runForRuleset(new TaikoRuleset().RulesetInfo);

        [Test]
        public void TestCatch() => runForRuleset(new CatchRuleset().RulesetInfo);

        [Test]
        public void TestMania() => runForRuleset(new ManiaRuleset().RulesetInfo);

        private void runForRuleset(RulesetInfo rulesetInfo)
        {
            AddStep("create screen", () =>
            {
                editorBeatmap.BeatmapInfo.Ruleset = rulesetInfo;

                Beatmap.Value = CreateWorkingBeatmap(editorBeatmap.PlayableBeatmap);

                Child = new SetupScreen
                {
                    State = { Value = Visibility.Visible },
                };
            });
        }
    }
}
