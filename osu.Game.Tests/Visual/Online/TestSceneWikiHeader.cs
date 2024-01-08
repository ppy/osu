// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Wiki;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneWikiHeader : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Orange);

        [Cached]
        private readonly Bindable<APIWikiPage> wikiPageData = new Bindable<APIWikiPage>(new APIWikiPage
        {
            Title = "Main page",
            Path = "Main_page",
        });

        private TestHeader header;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = header = new TestHeader
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                ShowIndexPage = dummyShowIndexPage,
                ShowParentPage = dummyShowParentPage,
            };
            wikiPageData.BindTo(header.WikiPageData);
        });

        [Test]
        public void TestWikiHeader()
        {
            AddAssert("Current is index", () => checkCurrent(WikiHeader.IndexPageString));

            AddStep("Change wiki page data", () => wikiPageData.Value = new APIWikiPage
            {
                Title = "Welcome",
                Path = "Welcome"
            });
            AddAssert("Current is welcome", () => checkCurrent("Welcome"));
            AddAssert("Check breadcrumb", checkBreadcrumb);

            AddStep("Change current to index", () => header.Current.Value = WikiHeader.IndexPageString);
            AddAssert("Current is index", () => checkCurrent(WikiHeader.IndexPageString));

            AddStep("Change wiki page data", () => wikiPageData.Value = new APIWikiPage
            {
                Title = "Developers",
                Path = "People/The_Team/Developers",
                Subtitle = "The Team",
            });
            AddAssert("Current is 'Developers'", () => checkCurrent("Developers"));
            AddAssert("Check breadcrumb", checkBreadcrumb);

            AddStep("Change current to 'The Team'", () => header.Current.Value = "The Team");
            AddAssert("Current is 'The Team'", () => checkCurrent("The Team"));
            AddAssert("Check breadcrumb", checkBreadcrumb);
        }

        private bool checkCurrent(LocalisableString expectedCurrent) => header.Current.Value == expectedCurrent;

        private bool checkBreadcrumb()
        {
            bool result = header.TabControlItems.Contains(wikiPageData.Value.Title);

            if (wikiPageData.Value.Subtitle != null)
                result = header.TabControlItems.Contains(wikiPageData.Value.Subtitle) && result;

            return result;
        }

        private void dummyShowIndexPage() => wikiPageData.SetDefault();

        private void dummyShowParentPage()
        {
            wikiPageData.Value = new APIWikiPage
            {
                Path = "People/The_Team",
                Title = "The Team",
                Subtitle = "People"
            };
        }

        private partial class TestHeader : WikiHeader
        {
            public IReadOnlyList<LocalisableString?> TabControlItems => TabControl.Items;
        }
    }
}
