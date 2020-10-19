// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public class TestSceneEditorSummaryTimeline : EditorClockTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            Add(new SummaryTimeline
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500, 50)
            });
        }
    }
}
