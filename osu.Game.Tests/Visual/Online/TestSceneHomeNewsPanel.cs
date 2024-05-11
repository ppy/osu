// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using System;
using osu.Game.Overlays.Dashboard.Home.News;
using osuTK;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneHomeNewsPanel : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Purple);

        public TestSceneHomeNewsPanel()
        {
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 500,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    new FeaturedNewsItemPanel(new APINewsPost
                    {
                        Title = "This post has an image which starts with \"/\" and has many authors!",
                        Preview = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                        FirstImage = "/help/wiki/shared/news/banners/monthly-beatmapping-contest.png",
                        PublishedAt = DateTimeOffset.Now,
                        Slug = "2020-07-16-summer-theme-park-2020-voting-open"
                    }),
                    new NewsItemGroupPanel(new List<APINewsPost>
                    {
                        new APINewsPost
                        {
                            Title = "Title 1",
                            Slug = "2020-07-16-summer-theme-park-2020-voting-open",
                            PublishedAt = DateTimeOffset.Now,
                        },
                        new APINewsPost
                        {
                            Title = "Title of this post is Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                            Slug = "2020-07-16-summer-theme-park-2020-voting-open",
                            PublishedAt = DateTimeOffset.Now,
                        }
                    }),
                    new ShowMoreNewsPanel()
                }
            });
        }
    }
}
