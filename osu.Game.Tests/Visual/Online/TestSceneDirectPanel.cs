// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneDirectPanel : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DirectGridPanel),
            typeof(DirectListPanel),
            typeof(IconPill)
        };

        private BeatmapSetInfo getBeatmapSet(RulesetInfo ruleset, bool downloadable)
        {
            var beatmap = CreateWorkingBeatmap(ruleset).BeatmapSetInfo;
            beatmap.OnlineInfo.HasVideo = true;
            beatmap.OnlineInfo.HasStoryboard = true;

            beatmap.OnlineInfo.Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = !downloadable,
                ExternalLink = "http://localhost",
            };

            return beatmap;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var ruleset = new OsuRuleset().RulesetInfo;

            var normal = CreateWorkingBeatmap(ruleset).BeatmapSetInfo;
            normal.OnlineInfo.HasVideo = true;
            normal.OnlineInfo.HasStoryboard = true;

            var downloadable = getBeatmapSet(ruleset, true);
            var undownloadable = getBeatmapSet(ruleset, false);

            DirectPanel undownloadableGridPanel, undownloadableListPanel;

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(20),
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    new DirectGridPanel(normal),
                    new DirectGridPanel(downloadable),
                    undownloadableGridPanel = new DirectGridPanel(undownloadable),
                    new DirectListPanel(normal),
                    new DirectListPanel(downloadable),
                    undownloadableListPanel = new DirectListPanel(undownloadable),
                },
            };

            AddAssert("is download button disabled on last grid panel", () => !undownloadableGridPanel.DownloadButton.Enabled.Value);
            AddAssert("is download button disabled on last list panel", () => !undownloadableListPanel.DownloadButton.Enabled.Value);
        }
    }
}
