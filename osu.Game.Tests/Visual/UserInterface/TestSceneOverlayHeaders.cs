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
            typeof(ControllableOverlayHeader),
            typeof(TabControlOverlayHeader),
            typeof(BreadcrumbControlOverlayHeader),
            typeof(TestNoControlHeader),
            typeof(TestTabControlHeader),
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

            addHeader("OverlayHeader", new TestNoControlHeader());
            addHeader("TabControlOverlayHeader", new TestTabControlHeader());
            addHeader("BreadcrumbControlOverlayHeader", new TestBreadcrumbControlHeader());
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
                        Margin = new MarginPadding(20),
                        Text = name,
                    },
                    header
                }
            });
        }

        private class TestNoControlHeader : OverlayHeader
        {
            protected override Drawable CreateBackground() => new TestBackground();

            protected override ScreenTitle CreateTitle() => new TestTitle();

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                TitleBackgroundColour = colours.GreyVioletDarker;
            }
        }

        private class TestTabControlHeader : TabControlOverlayHeader
        {
            protected override Drawable CreateBackground() => new TestBackground();

            protected override ScreenTitle CreateTitle() => new TestTitle();

            public TestTabControlHeader()
            {
                TabControl.AddItem("tab1");
                TabControl.AddItem("tab2");
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                TitleBackgroundColour = colours.GreyVioletDarker;
                ControlBackgroundColour = colours.GreyVioletDark;
                TabControl.AccentColour = colours.Violet;
            }
        }

        private class TestBreadcrumbControlHeader : BreadcrumbControlOverlayHeader
        {
            protected override Drawable CreateBackground() => new TestBackground();

            protected override ScreenTitle CreateTitle() => new TestTitle();

            public TestBreadcrumbControlHeader()
            {
                BreadcrumbControl.AddItem("tab1");
                BreadcrumbControl.AddItem("tab2");
                BreadcrumbControl.Current.Value = "tab2";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                TitleBackgroundColour = colours.GreyVioletDarker;
                ControlBackgroundColour = colours.GreyVioletDark;
                BreadcrumbControl.AccentColour = colours.Violet;
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

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Violet;
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }
}
