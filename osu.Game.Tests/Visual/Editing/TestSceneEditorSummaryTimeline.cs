// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneEditorSummaryTimeline : EditorClockTestScene
    {
        [Cached(typeof(EditorBeatmap))]
        private readonly EditorBeatmap editorBeatmap;

        public TestSceneEditorSummaryTimeline()
        {
            var beatmap = CreateBeatmap(new OsuRuleset().RulesetInfo);

            beatmap.ControlPointInfo.Add(100000, new TimingControlPoint { BeatLength = 100 });
            beatmap.ControlPointInfo.Add(50000, new DifficultyControlPoint { SliderVelocity = 2 });
            beatmap.BeatmapInfo.Bookmarks = new[] { 75000, 125000 };

            editorBeatmap = new EditorBeatmap(beatmap);
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
