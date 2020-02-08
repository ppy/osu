// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOverlayHeader : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OverlayHeader),
            typeof(TabControlOverlayHeader<>),
            typeof(BreadcrumbControlOverlayHeader),
            typeof(TestNoControlHeader),
            typeof(TestStringTabControlHeader),
            typeof(TestEnumTabControlHeader),
            typeof(TestBreadcrumbControlHeader),
            typeof(OverlayHeaderBackground)
        };

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

            addHeader("Orange OverlayHeader (no background)", new TestNoBackgroundHeader(), OverlayColourScheme.Orange);
            addHeader("Blue OverlayHeader", new TestNoControlHeader(), OverlayColourScheme.Blue);
            addHeader("Green TabControlOverlayHeader (string) with ruleset selector", new TestStringTabControlHeader(), OverlayColourScheme.Green);
            addHeader("Pink TabControlOverlayHeader (enum)", new TestEnumTabControlHeader(), OverlayColourScheme.Pink);
            addHeader("Red BreadcrumbControlOverlayHeader (no background)", new TestBreadcrumbControlHeader(), OverlayColourScheme.Red);
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

        private class ColourProvidedContainer : Container
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

        private class TestNoBackgroundHeader : OverlayHeader
        {
            protected override ScreenTitle CreateTitle() => new TestTitle();
        }

        private class TestNoControlHeader : OverlayHeader
        {
            protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/changelog");

            protected override ScreenTitle CreateTitle() => new TestTitle();
        }

        private class TestStringTabControlHeader : TabControlOverlayHeader<string>
        {
            protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/news");

            protected override ScreenTitle CreateTitle() => new TestTitle();

            protected override Drawable CreateTitleContent() => new OverlayRulesetSelector();

            public TestStringTabControlHeader()
            {
                TabControl.AddItem("tab1");
                TabControl.AddItem("tab2");
            }
        }

        private class TestEnumTabControlHeader : TabControlOverlayHeader<TestEnum>
        {
            protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/rankings");

            protected override ScreenTitle CreateTitle() => new TestTitle();
        }

        private enum TestEnum
        {
            Some,
            Cool,
            Tabs
        }

        private class TestBreadcrumbControlHeader : BreadcrumbControlOverlayHeader
        {
            protected override ScreenTitle CreateTitle() => new TestTitle();

            public TestBreadcrumbControlHeader()
            {
                TabControl.AddItem("tab1");
                TabControl.AddItem("tab2");
                TabControl.Current.Value = "tab2";
            }
        }

        private class TestTitle : ScreenTitle
        {
            public TestTitle()
            {
                Title = "title";
                Section = "section";
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }
}
