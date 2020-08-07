// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using System;
using osu.Game.Overlays.Dashboard.Home.News;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneHomeNewsPanel : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Purple);

        public TestSceneHomeNewsPanel()
        {
            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 500,
                Child = new HomeNewsPanel(new APINewsPost
                {
                    Title = "This post has an image which starts with \"/\" and has many authors!",
                    Preview = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                    FirstImage = "/help/wiki/shared/news/banners/monthly-beatmapping-contest.png",
                    PublishedAt = DateTimeOffset.Now,
                    Slug = "2020-07-16-summer-theme-park-2020-voting-open"
                })
            });
        }
    }
}
