// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.Dashboard.Home;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using System;
using osu.Framework.Graphics.Shapes;
using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneDashboardBeatmapListing : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private readonly Container content;

        public TestSceneDashboardBeatmapListing()
        {
            Add(content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 300,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = 10 },
                        Child = new DashboardBeatmapListing(new_beatmaps, popular_beatmaps)
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Set width to 500", () => content.ResizeWidthTo(500, 500));
            AddStep("Set width to 300", () => content.ResizeWidthTo(300, 500));
        }

        private static readonly List<APIBeatmapSet> new_beatmaps = new List<APIBeatmapSet>
        {
            new APIBeatmapSet
            {
                Title = "Very Long Title (TV size) [TATOE]",
                Artist = "This artist has a really long name how is this possible",
                Author = new APIUser
                {
                    Username = "author",
                    Id = 100
                },
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = "https://assets.ppy.sh/beatmaps/1189904/covers/cover.jpg?1595456608",
                },
                Ranked = DateTimeOffset.Now
            },
            new APIBeatmapSet
            {
                Title = "Very Long Title (TV size) [TATOE]",
                Artist = "This artist has a really long name how is this possible",
                Author = new APIUser
                {
                    Username = "author",
                    Id = 100
                },
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = "https://assets.ppy.sh/beatmaps/1189904/covers/cover.jpg?1595456608",
                },
                Ranked = DateTimeOffset.Now
            }
        };

        private static readonly List<APIBeatmapSet> popular_beatmaps = new List<APIBeatmapSet>
        {
            new APIBeatmapSet
            {
                Title = "Very Long Title (TV size) [TATOE]",
                Artist = "This artist has a really long name how is this possible",
                Author = new APIUser
                {
                    Username = "author",
                    Id = 100
                },
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = "https://assets.ppy.sh/beatmaps/1189904/covers/cover.jpg?1595456608",
                },
                Ranked = DateTimeOffset.Now
            },
            new APIBeatmapSet
            {
                Title = "Very Long Title (TV size) [TATOE]",
                Artist = "This artist has a really long name how is this possible",
                Author = new APIUser
                {
                    Username = "author",
                    Id = 100
                },
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = "https://assets.ppy.sh/beatmaps/1189904/covers/cover.jpg?1595456608",
                },
                Ranked = DateTimeOffset.Now
            }
        };
    }
}
