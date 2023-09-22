// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneBreadcrumbControl : OsuTestScene
    {
        private readonly TestBreadcrumbControl breadcrumbs;

        public TestSceneBreadcrumbControl()
        {
            Add(breadcrumbs = new TestBreadcrumbControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Width = 0.5f,
            });

            AddStep(@"first", () => breadcrumbs.Current.Value = BreadcrumbTab.Click);
            assertVisible(1);

            AddStep(@"second", () => breadcrumbs.Current.Value = BreadcrumbTab.The);
            assertVisible(2);

            AddStep(@"third", () => breadcrumbs.Current.Value = BreadcrumbTab.Circles);
            assertVisible(3);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            breadcrumbs.StripColour = colours.Blue;
        }

        private void assertVisible(int count) => AddAssert($"first {count} item(s) visible", () =>
        {
            for (int i = 0; i < count; i++)
            {
                if (breadcrumbs.GetDrawable((BreadcrumbTab)i).State != Visibility.Visible)
                    return false;
            }

            return true;
        });

        private enum BreadcrumbTab
        {
            Click,
            The,
            Circles,
        }

        private partial class TestBreadcrumbControl : BreadcrumbControl<BreadcrumbTab>
        {
            public BreadcrumbTabItem GetDrawable(BreadcrumbTab tab) => (BreadcrumbTabItem)TabContainer.First(t => t.Value == tab);
        }
    }
}
