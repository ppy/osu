// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogSupporterPromo : CompositeDrawable
    {
        private const float image_container_width = 164;
        private const float heart_size = 75;

        public ChangelogSupporterPromo()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding
            {
                Vertical = 20,
                Horizontal = 50,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, TextureStore textures, OverlayColourProvider colourProvider)
        {
            SupporterPromoLinkFlowContainer supportLinkText;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    CornerRadius = 6,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Offset = new Vector2(0, 1),
                        Radius = 3,
                    },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 200,
                            Padding = new MarginPadding { Horizontal = 75 },
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Padding = new MarginPadding { Right = 50 + image_container_width },
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = ChangelogStrings.SupportHeading,
                                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Light),
                                            Margin = new MarginPadding { Bottom = 20 },
                                        },
                                        supportLinkText = new SupporterPromoLinkFlowContainer(t =>
                                        {
                                            t.Font = t.Font.With(size: 14);
                                            t.Colour = colour.PinkLighter;
                                        })
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                        },
                                        new OsuTextFlowContainer(t =>
                                        {
                                            t.Font = t.Font.With(size: 12);
                                            t.Colour = colour.PinkLighter;
                                        })
                                        {
                                            Text = ChangelogStrings.SupportText2,
                                            Margin = new MarginPadding { Top = 10 },
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                        }
                                    },
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = image_container_width,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Children = new Drawable[]
                                    {
                                        new Sprite
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Margin = new MarginPadding { Bottom = 28 },
                                            RelativeSizeAxes = Axes.Both,
                                            FillMode = FillMode.Fill,
                                            Texture = textures.Get(@"Online/supporter-pippi"),
                                        },
                                        new Container
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Size = new Vector2(heart_size),
                                            Margin = new MarginPadding { Top = 70 },
                                            Masking = true,
                                            EdgeEffect = new EdgeEffectParameters
                                            {
                                                Type = EdgeEffectType.Shadow,
                                                Colour = colour.Pink,
                                                Radius = 10,
                                                Roundness = heart_size / 2,
                                            },
                                            Child = new Sprite
                                            {
                                                Size = new Vector2(heart_size),
                                                Texture = textures.Get(@"Online/supporter-heart"),
                                            },
                                        },
                                    }
                                }
                            }
                        },
                    }
                },
            };

            supportLinkText.AddText("Support further development of osu! and ");
            supportLinkText.AddLink("become an osu!supporter", @"https://osu.ppy.sh/home/support", t => t.Font = t.Font.With(weight: FontWeight.Bold));
            supportLinkText.AddText(" today!");
        }

        private class SupporterPromoLinkFlowContainer : LinkFlowContainer
        {
            public SupporterPromoLinkFlowContainer(Action<SpriteText> defaultCreationParameters)
                : base(defaultCreationParameters)
            {
            }

            protected override DrawableLinkCompiler CreateLinkCompiler(ITextPart textPart) => new SupporterPromoLinkCompiler(textPart);

            private class SupporterPromoLinkCompiler : DrawableLinkCompiler
            {
                public SupporterPromoLinkCompiler(ITextPart part)
                    : base(part)
                {
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colour)
                {
                    IdleColour = colour.PinkDark;
                    HoverColour = Color4.White;
                }
            }
        }
    }
}
