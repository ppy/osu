// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.Dashboard.Home;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using osu.Game.Users;
using System;
using osu.Framework.Graphics.Shapes;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneDashboardBeatmapListing : OsuTestScene
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

        private static readonly List<BeatmapSetInfo> new_beatmaps = new List<BeatmapSetInfo>
        {
            new BeatmapSetInfo
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
            },
            new BeatmapSetInfo
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
                    Ranked = DateTimeOffset.MinValue
                }
            }
        };

        private static readonly List<BeatmapSetInfo> popular_beatmaps = new List<BeatmapSetInfo>
        {
            new BeatmapSetInfo
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
                        Cover = "https://assets.ppy.sh/beatmaps/1079428/covers/cover.jpg?1595295586",
                    },
                    FavouriteCount = 100
                }
            },
            new BeatmapSetInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Title = "Title 2",
                    Artist = "Artist 2",
                    Author = new User
                    {
                        Username = "someone",
                        Id = 100
                    }
                },
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Covers = new BeatmapSetOnlineCovers
                    {
                        Cover = "https://assets.ppy.sh/beatmaps/1079428/covers/cover.jpg?1595295586",
                    },
                    FavouriteCount = 10
                }
            }
        };
    }
}
