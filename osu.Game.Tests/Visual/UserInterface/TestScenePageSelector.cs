// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface.PageSelector;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestScenePageSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(PageSelector),
            typeof(DrawablePage),
            typeof(PageSelectorButton),
            typeof(PageSelectorItem)
        };

        private readonly PageSelector pageSelector;
        private readonly DrawablePage drawablePage;

        public TestScenePageSelector()
        {
            AddRange(new Drawable[]
            {
                pageSelector = new PageSelector
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                drawablePage = new DrawablePage(1234)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = 50 },
                }
            });
        }

        [Test]
        public void TestPageSelectorValues()
        {
            AddStep("10 max pages", () => setMaxPages(10));
            AddStep("11 max pages", () => setMaxPages(11));
            AddStep("200 max pages, current 199", () =>
            {
                setMaxPages(200);
                setCurrentPage(199);
            });
            AddStep("200 max pages, current 201", () =>
            {
                setMaxPages(200);
                setCurrentPage(201);
            });
            AddAssert("Current equals max", () => pageSelector.CurrentPage.Value == pageSelector.MaxPages.Value);
            AddStep("200 max pages, current -10", () =>
            {
                setMaxPages(200);
                setCurrentPage(-10);
            });
            AddAssert("Current is 1", () => pageSelector.CurrentPage.Value == 1);
            AddStep("-10 max pages", () =>
            {
                setMaxPages(-10);
            });
            AddAssert("Current is 1, max is 1", () => pageSelector.CurrentPage.Value == 1 && pageSelector.MaxPages.Value == 1);
        }

        [Test]
        public void TestDrawablePage()
        {
            AddStep("Select", () => drawablePage.Selected = true);
            AddStep("Deselect", () => drawablePage.Selected = false);
        }

        private void setMaxPages(int maxPages) => pageSelector.MaxPages.Value = maxPages;

        private void setCurrentPage(int currentPage) => pageSelector.CurrentPage.Value = currentPage;
    }
}
