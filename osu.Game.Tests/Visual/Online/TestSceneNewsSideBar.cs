// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.News.Sidebar;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneNewsSidebar : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private NewsSidebar sidebar;

        [SetUp]
        public void SetUp() => Schedule(() => Child = sidebar = new NewsSidebar());

        [Test]
        public void TestMetadataWithNoPosts()
        {
            AddStep("Add data with no posts", () => sidebar.Metadata.Value = metadata_with_no_posts);
            AddUntilStep("No dropdowns were created", () => !sidebar.ChildrenOfType<MonthSection>().Any());
        }

        [Test]
        public void TestYearsPanelVisibility()
        {
            AddUntilStep("Years panel is hidden", () => yearsPanel?.Alpha == 0);
            AddStep("Add data", () => sidebar.Metadata.Value = metadata);
            AddUntilStep("Years panel is visible", () => yearsPanel?.Alpha == 1);
        }

        private YearsPanel yearsPanel => sidebar.ChildrenOfType<YearsPanel>().FirstOrDefault();

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

        private static readonly APINewsSidebar metadata_with_no_posts = new APINewsSidebar
        {
            CurrentYear = 2022,
            Years = new[]
            {
                2022,
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
            NewsPosts = Array.Empty<APINewsPost>()
        };
    }
}
