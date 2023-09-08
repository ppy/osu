// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.News;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osuTK;
using System;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneNewsCard : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Purple);

        public TestSceneNewsCard()
        {
            Add(new FillFlowContainer<NewsCard>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Vertical,
                Width = 500,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 20),
                Children = new[]
                {
                    new NewsCard(new APINewsPost
                    {
                        Title = "This post has an image which starts with \"/\" and has many authors! (clickable)",
                        Preview = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                        Author = "someone, someone1, someone2, someone3, someone4",
                        FirstImage = "/help/wiki/shared/news/banners/monthly-beatmapping-contest.png",
                        PublishedAt = DateTimeOffset.Now,
                        Slug = "2020-07-16-summer-theme-park-2020-voting-open"
                    }),
                    new NewsCard(new APINewsPost
                    {
                        Title = "This post has a full-url image! (HTML entity: &amp;) (non-clickable)",
                        Preview = "boom (HTML entity: &amp;)",
                        Author = "user (HTML entity: &amp;)",
                        FirstImage = "https://assets.ppy.sh/artists/88/header.jpg",
                        PublishedAt = DateTimeOffset.Now
                    })
                }
            });
        }
    }
}
