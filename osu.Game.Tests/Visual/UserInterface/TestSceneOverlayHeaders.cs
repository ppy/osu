// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOverlayHeaders : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OverlayHeader),
            typeof(ControllableOverlayHeader<>),
            typeof(TabControlOverlayHeader<>),
            typeof(BreadcrumbControlOverlayHeader),
            typeof(TestNoControlHeader),
            typeof(TestStringTabControlHeader),
            typeof(TestEnumTabControlHeader),
            typeof(TestBreadcrumbControlHeader),
        };

        private readonly FillFlowContainer flow;

        public TestSceneOverlayHeaders()
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

            addHeader("Blue OverlayHeader", new TestNoControlHeader(OverlayColourScheme.Blue));
            addHeader("Green TabControlOverlayHeader (string)", new TestStringTabControlHeader(OverlayColourScheme.Green));
            addHeader("Pink TabControlOverlayHeader (enum)", new TestEnumTabControlHeader(OverlayColourScheme.Pink));
            addHeader("Red BreadcrumbControlOverlayHeader", new TestBreadcrumbControlHeader(OverlayColourScheme.Red));
        }

        private void addHeader(string name, OverlayHeader header)
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
                    header.With(h =>
                    {
                        h.Anchor = Anchor.TopCentre;
                        h.Origin = Anchor.TopCentre;
                    })
                }
            });
        }

        private class TestNoControlHeader : OverlayHeader
        {
            protected override Drawable CreateBackground() => new TestBackground();

            protected override ScreenTitle CreateTitle() => new TestTitle();

            public TestNoControlHeader(OverlayColourScheme colourScheme)
                : base(colourScheme)
            {
            }
        }

        private class TestStringTabControlHeader : TabControlOverlayHeader<string>
        {
            protected override Drawable CreateBackground() => new TestBackground();

            protected override ScreenTitle CreateTitle() => new TestTitle();

            public TestStringTabControlHeader(OverlayColourScheme colourScheme)
                : base(colourScheme)
            {
                TabControl.AddItem("tab1");
                TabControl.AddItem("tab2");
            }
        }

        private class TestEnumTabControlHeader : TabControlOverlayHeader<TestEnum>
        {
            public TestEnumTabControlHeader(OverlayColourScheme colourScheme)
                : base(colourScheme)
            {
            }

            protected override Drawable CreateBackground() => new TestBackground();

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
            protected override Drawable CreateBackground() => new TestBackground();

            protected override ScreenTitle CreateTitle() => new TestTitle();

            public TestBreadcrumbControlHeader(OverlayColourScheme colourScheme)
                : base(colourScheme)
            {
                BreadcrumbControl.AddItem("tab1");
                BreadcrumbControl.AddItem("tab2");
                BreadcrumbControl.Current.Value = "tab2";
            }
        }

        private class TestBackground : Sprite
        {
            public TestBackground()
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(@"Headers/changelog");
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
