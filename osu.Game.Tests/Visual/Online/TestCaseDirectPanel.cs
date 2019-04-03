// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestCaseDirectPanel : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DirectGridPanel),
            typeof(DirectListPanel),
            typeof(IconPill)
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            var beatmap = new TestWorkingBeatmap(new OsuRuleset().RulesetInfo, null);
            beatmap.BeatmapSetInfo.OnlineInfo.HasVideo = true;
            beatmap.BeatmapSetInfo.OnlineInfo.HasStoryboard = true;

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(20),
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    new DirectGridPanel(beatmap.BeatmapSetInfo),
                    new DirectListPanel(beatmap.BeatmapSetInfo)
                }
            };
        }
    }
}
