// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using OpenTK;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Framework.Configuration;
using osu.Framework.Timing;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorSummaryTimeline : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(SummaryTimeline) };

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(parent);

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmap.Value = new TestWorkingBeatmap(new OsuRuleset().RulesetInfo);

            var clock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            dependencies.CacheAs<IAdjustableClock>(clock);
            dependencies.CacheAs<IFrameBasedClock>(clock);

            SummaryTimeline summaryTimeline;
            Add(summaryTimeline = new SummaryTimeline
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500, 50)
            });

            summaryTimeline.Beatmap.BindTo(beatmap);
        }
    }
}
