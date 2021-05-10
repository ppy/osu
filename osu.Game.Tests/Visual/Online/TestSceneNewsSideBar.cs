// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.News.Sidebar;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneNewsSideBar : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private NewsSideBar sidebar;

        [Test]
        public void TestCreateEmpty()
        {
            createSidebar(null);
        }

        [Test]
        public void TestCreateWithData()
        {
            createSidebar(metadata);
        }

        [Test]
        public void TestDataChange()
        {
            createSidebar(null);
            AddStep("Add data", () =>
            {
                if (sidebar != null)
                    sidebar.Metadata.Value = metadata;
            });
        }

        private void createSidebar(APINewsSidebar metadata) => AddStep("Create", () => Child = sidebar = new NewsSideBar
        {
            Metadata = { Value = metadata }
        });

        private static readonly APINewsSidebar metadata = new APINewsSidebar
        {
            CurrentYear = 2021,
            Years = new[]
            {
                2021,
                2020,
                2019,
                2018,
                2017,
                2016,
                2015,
                2014,
                2013
            },
            NewsPosts = new List<APINewsPost>
            {
                new APINewsPost
                {
                    Title = "(Mar) Short title",
                    PublishedAt = new DateTime(2021, 3, 1)
                },
                new APINewsPost
                {
                    Title = "(Mar) Oh boy that's a long post title I wonder if it will break anything",
                    PublishedAt = new DateTime(2021, 3, 1)
                },
                new APINewsPost
                {
                    Title = "(Mar) Medium title, nothing to see here",
                    PublishedAt = new DateTime(2021, 3, 1)
                },
                new APINewsPost
                {
                    Title = "(Feb) Short title",
                    PublishedAt = new DateTime(2021, 2, 1)
                },
                new APINewsPost
                {
                    Title = "(Feb) Oh boy that's a long post title I wonder if it will break anything",
                    PublishedAt = new DateTime(2021, 2, 1)
                },
                new APINewsPost
                {
                    Title = "(Feb) Medium title, nothing to see here",
                    PublishedAt = new DateTime(2021, 2, 1)
                },
                new APINewsPost
                {
                    Title = "Short title",
                    PublishedAt = new DateTime(2021, 1, 1)
                },
                new APINewsPost
                {
                    Title = "Oh boy that's a long post title I wonder if it will break anything",
                    PublishedAt = new DateTime(2021, 1, 1)
                },
                new APINewsPost
                {
                    Title = "Medium title, nothing to see here",
                    PublishedAt = new DateTime(2021, 1, 1)
                }
            }
        };
    }
}
