// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public class TestSceneNewsMonthPanel : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Test]
        public void CreateClosedMonthPanel()
        {
            AddStep("Create", () => Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background2,
                    },
                    new MonthPanel(DateTime.Now, posts),
                }
            });
        }

        [Test]
        public void CreateOpenMonthPanel()
        {
            AddStep("Create", () => Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background2,
                    },
                    new MonthPanel(DateTime.Now, posts)
                    {
                        IsOpen = { Value = true }
                    },
                }
            });
        }

        private static APINewsPost[] posts => new[]
        {
            new APINewsPost
            {
                Title = "Short title"
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
