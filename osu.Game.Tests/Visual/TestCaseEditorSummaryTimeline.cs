// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osuTK;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorSummaryTimeline : EditorClockTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(SummaryTimeline) };

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = new TestWorkingBeatmap(new OsuRuleset().RulesetInfo);

            Add(new SummaryTimeline
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500, 50)
            });
        }
    }
}
