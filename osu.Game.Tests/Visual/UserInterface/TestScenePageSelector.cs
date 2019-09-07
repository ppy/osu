// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestScenePageSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(PageSelector)
        };

        public TestScenePageSelector()
        {
            Child = new PageSelector(200)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            AddStep("1 max pages", () => redraw(1));
            AddStep("10 max pages", () => redraw(10));
            AddStep("200 max pages, current 199", () => redraw(200, 199));
            AddStep("200 max pages, current 201", () => redraw(200, 201));
            AddStep("200 max pages, current -10", () => redraw(200, -10));
        }

        private void redraw(int maxPages, int currentPage = 0)
        {
            Clear();

            var selector = new PageSelector(maxPages)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            if (currentPage != 0)
                selector.CurrentPage.Value = currentPage;

            Add(selector);
        }
    }
}
