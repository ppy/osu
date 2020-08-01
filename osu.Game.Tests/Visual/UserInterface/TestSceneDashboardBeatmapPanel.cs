// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.Dashboard.Dashboard;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using osu.Game.Users;
using System;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneDashboardBeatmapPanel : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        public TestSceneDashboardBeatmapPanel()
        {
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 300,
                Spacing = new Vector2(0, 10),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new DashboardNewBeatmapPanel(new_beatmap_set),
                    new DashboardPopularBeatmapPanel(popular_beatmap_set)
                }
            });
        }

        private static readonly BeatmapSetInfo new_beatmap_set = new BeatmapSetInfo
        {
            Metadata = new BeatmapMetadata
            {
                Title = "Very Long Title (TV size) [TATOE]",
                Artist = "This artist has a really long name how is this possible",
                Author = new User
                {
                    Username = "author",
                    Id = 100
                }
            },
            OnlineInfo = new BeatmapSetOnlineInfo
            {
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = "https://assets.ppy.sh/beatmaps/1189904/covers/cover.jpg?1595456608",
                },
                Ranked = DateTimeOffset.Now
            }
        };

        private static readonly BeatmapSetInfo popular_beatmap_set = new BeatmapSetInfo
        {
            Metadata = new BeatmapMetadata
            {
                Title = "Title",
                Artist = "Artist",
                Author = new User
                {
                    Username = "author",
                    Id = 100
                }
            },
            OnlineInfo = new BeatmapSetOnlineInfo
            {
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = "https://assets.ppy.sh/beatmaps/1189904/covers/cover.jpg?1595456608",
                },
                FavouriteCount = 100
            }
        };
    }
}
