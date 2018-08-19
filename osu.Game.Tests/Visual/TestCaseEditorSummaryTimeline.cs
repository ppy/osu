// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using OpenTK;
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
