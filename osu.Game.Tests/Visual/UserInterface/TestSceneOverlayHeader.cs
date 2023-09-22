// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOverlayHeader : OsuTestScene
    {
        private readonly FillFlowContainer flow;

        public TestSceneOverlayHeader()
        {
            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = flow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical
                    }
                }
            });

            addHeader("Orange OverlayHeader (no background, 100 padding)", new TestNoBackgroundHeader(), OverlayColourScheme.Orange);
            addHeader("Blue OverlayHeader (default 50 padding)", new TestNoControlHeader(), OverlayColourScheme.Blue);
            addHeader("Green TabControlOverlayHeader (string) with ruleset selector", new TestStringTabControlHeader(), OverlayColourScheme.Green);
            addHeader("Pink TabControlOverlayHeader (enum, 30 padding)", new TestEnumTabControlHeader(), OverlayColourScheme.Pink);
            addHeader("Red BreadcrumbControlOverlayHeader (no background, 10 padding)", new TestBreadcrumbControlHeader(), OverlayColourScheme.Red);
        }

        private void addHeader(string name, OverlayHeader header, OverlayColourScheme colourScheme)
        {
            flow.Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding(20),
                        Text = name,
                    },
                    new ColourProvidedContainer(colourScheme, header)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    }
                }
            });
        }

        private partial class ColourProvidedContainer : Container
        {
            [Cached]
            private readonly OverlayColourProvider colourProvider;

            public ColourProvidedContainer(OverlayColourScheme colourScheme, OverlayHeader header)
            {
                colourProvider = new OverlayColourProvider(colourScheme);

                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                Add(header);
            }
        }

        private partial class TestNoBackgroundHeader : OverlayHeader
        {
            protected override OverlayTitle CreateTitle() => new TestTitle();

            public TestNoBackgroundHeader()
            {
                ContentSidePadding = 100;
            }
        }

        private partial class TestNoControlHeader : OverlayHeader
        {
            protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/changelog");

            protected override OverlayTitle CreateTitle() => new TestTitle();
        }

        private partial class TestStringTabControlHeader : TabControlOverlayHeader<string>
        {
            protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/news");

            protected override OverlayTitle CreateTitle() => new TestTitle();

            protected override Drawable CreateTabControlContent() => new OverlayRulesetSelector();

            public TestStringTabControlHeader()
            {
                TabControl.AddItem("tab1");
                TabControl.AddItem("tab2");
            }
        }

        private partial class TestEnumTabControlHeader : TabControlOverlayHeader<TestEnum>
        {
            public TestEnumTabControlHeader()
            {
                ContentSidePadding = 30;
            }

            protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/rankings");

            protected override OverlayTitle CreateTitle() => new TestTitle();
        }

        private enum TestEnum
        {
            Some,
            Cool,
            Tabs
        }

        private partial class TestBreadcrumbControlHeader : BreadcrumbControlOverlayHeader
        {
            protected override OverlayTitle CreateTitle() => new TestTitle();

            public TestBreadcrumbControlHeader()
            {
                ContentSidePadding = 10;

                TabControl.AddItem("tab1");
                TabControl.AddItem("tab2");
                TabControl.Current.Value = "tab2";
            }
        }

        private partial class TestTitle : OverlayTitle
        {
            public TestTitle()
            {
                Title = "title";
                IconTexture = "Icons/changelog";
            }
        }
    }
}
