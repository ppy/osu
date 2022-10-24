// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public class TestSceneEditorSummaryTimeline : EditorClockTestScene
    {
        [Cached(typeof(EditorBeatmap))]
        private readonly EditorBeatmap editorBeatmap;

        public TestSceneEditorSummaryTimeline()
        {
            editorBeatmap = new EditorBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("create timeline", () =>
            {
                // required for track
                Beatmap.Value = CreateWorkingBeatmap(editorBeatmap.PlayableBeatmap);

                Add(new SummaryTimeline
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 50)
                });
            });
        }
    }
}
