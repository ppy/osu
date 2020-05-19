// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.SearchableList;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi
{
    public class Header : Container
    {
        public const float HEIGHT = 121;

        private readonly HeaderBreadcrumbControl breadcrumbs;

        public Header(ScreenStack stack)
        {
            MultiHeaderTitle title;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex(@"2f2043"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING + OsuScreen.HORIZONTAL_OVERFLOW_PADDING },
                    Children = new Drawable[]
                    {
                        title = new MultiHeaderTitle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.BottomLeft,
                            X = -MultiHeaderTitle.ICON_WIDTH,
                        },
                        breadcrumbs = new HeaderBreadcrumbControl(stack)
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
            };

            breadcrumbs.Current.ValueChanged += screen =>
            {
                if (screen.NewValue is IMultiplayerSubScreen multiScreen)
                    title.Screen = multiScreen;
            };

            breadcrumbs.Current.TriggerChange();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            breadcrumbs.StripColour = colours.Green;
        }

        private class MultiHeaderTitle : CompositeDrawable, IHasAccentColour
        {
            public const float ICON_WIDTH = icon_size + spacing;

            private const float icon_size = 25;
            private const float spacing = 6;
            private const int text_offset = 2;

            private readonly SpriteIcon iconSprite;
            private readonly OsuSpriteText title, pageText;

            public IMultiplayerSubScreen Screen
            {
                set => pageText.Text = value.ShortTitle.ToLowerInvariant();
            }

            public Color4 AccentColour
            {
                get => pageText.Colour;
                set => pageText.Colour = value;
            }

            public MultiHeaderTitle()
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(spacing, 0),
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            iconSprite = new SpriteIcon
                            {
                                Size = new Vector2(icon_size),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            },
                            title = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                                Margin = new MarginPadding { Bottom = text_offset }
                            },
                            new Circle
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(4),
                                Colour = Color4.Gray,
                            },
                            pageText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.GetFont(size: 20),
                                Margin = new MarginPadding { Bottom = text_offset }
                            }
                        }
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                title.Text = "multi";
                iconSprite.Icon = OsuIcon.Multi;
                AccentColour = colours.Yellow;
            }
        }

        private class HeaderBreadcrumbControl : ScreenBreadcrumbControl
        {
            public HeaderBreadcrumbControl(ScreenStack stack)
                : base(stack)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                AccentColour = Color4.White;
            }
        }
    }
}
