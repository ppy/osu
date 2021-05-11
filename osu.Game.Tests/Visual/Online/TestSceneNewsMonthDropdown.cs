// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.News.Sidebar;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneNewsMonthDropdown : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Test]
        public void CreateClosedMonthPanel()
        {
            create(false);
        }

        [Test]
        public void CreateOpenMonthPanel()
        {
            create(true);
        }

        private void create(bool isOpen) => AddStep("Create", () => Child = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.TopCentre,
            AutoSizeAxes = Axes.Y,
            Width = 160,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background2,
                },
                new MonthDropdown(posts)
                {
                    IsOpen = { Value = isOpen }
                }
            }
        });

        private static List<APINewsPost> posts => new List<APINewsPost>
        {
            new APINewsPost
            {
                Title = "Short title",
                PublishedAt = DateTimeOffset.Now
            },
            new APINewsPost
            {
                Title = "Oh boy that's a long post title I wonder if it will break anything"
            },
            new APINewsPost
            {
                Title = "Medium title, nothing to see here"
            }
        };
    }
}
