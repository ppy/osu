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

        private BeatmapSetInfo getUndownloadableBeatmapSet(RulesetInfo ruleset)
        {
            var beatmap = CreateWorkingBeatmap(ruleset).BeatmapSetInfo;
            beatmap.Metadata.Artist = "test";
            beatmap.Metadata.Title = "undownloadable";
            beatmap.Metadata.AuthorString = "test";

            beatmap.OnlineInfo.HasVideo = true;
            beatmap.OnlineInfo.HasStoryboard = true;

            beatmap.OnlineInfo.Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = true,
                ExternalLink = "http://osu.ppy.sh",
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

            var undownloadable = getUndownloadableBeatmapSet(ruleset);
            DirectPanel undownloadableGridPanel, undownloadableListPanel;

            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
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
                        new DirectListPanel(normal),
                        undownloadableGridPanel = new DirectGridPanel(undownloadable),
                        undownloadableListPanel = new DirectListPanel(undownloadable),
                    },
                },
            };

            AddAssert("is download button disabled on second grid panel", () => !undownloadableGridPanel.DownloadButton.Enabled.Value);
            AddAssert("is download button disabled on second list panel", () => !undownloadableListPanel.DownloadButton.Enabled.Value);
        }
    }
}
