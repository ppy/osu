// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneBreadcrumbs : OsuTestScene
    {
        private readonly BreadcrumbControl<BreadcrumbTab> breadcrumbs;

        public TestSceneBreadcrumbs()
        {
            Add(breadcrumbs = new BreadcrumbControl<BreadcrumbTab>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Width = 0.5f,
            });

            AddStep(@"first", () => breadcrumbs.Current.Value = BreadcrumbTab.Click);
            AddStep(@"second", () => breadcrumbs.Current.Value = BreadcrumbTab.The);
            AddStep(@"third", () => breadcrumbs.Current.Value = BreadcrumbTab.Circles);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            breadcrumbs.StripColour = colours.Blue;
        }

        private enum BreadcrumbTab
        {
            Click,
            The,
            Circles,
        }
    }
}
