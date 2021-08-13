// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        private readonly FillFlowContainer textContainer;
        private readonly Container imageContainer;

        public ChangelogSupporterPromo()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding
            {
                Vertical = 20,
                Horizontal = 50,
            };

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
                            Colour = Color4.Black.Opacity(0.3f),
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 200,
                            Padding = new MarginPadding { Horizontal = 75 },
                            Children = new Drawable[]
                            {
                                textContainer = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Padding = new MarginPadding { Right = 50 + image_container_width },
                                },
                                imageContainer = new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = image_container_width,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                }
                            }
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, TextureStore textures)
        {
            SupporterPromoLinkFlowContainer supportLinkText;
            textContainer.Children = new Drawable[]
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
                    Text = ChangelogStrings.SupportText2.ToString(),
                    Margin = new MarginPadding { Top = 10 },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            };

            supportLinkText.AddText("Support further development of osu! and ");
            supportLinkText.AddLink("become and osu!supporter", "https://osu.ppy.sh/home/support", t => t.Font = t.Font.With(weight: FontWeight.Bold));
            supportLinkText.AddText(" today!");

            imageContainer.Children = new Drawable[]
            {
                new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    Texture = textures.Get(@"Online/supporter-pippi"),
                },
                new Sprite
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 75,
                    Height = 75,
                    Margin = new MarginPadding { Top = 70 },
                    Texture = textures.Get(@"Online/supporter-heart"),
                },
            };
        }

        private class SupporterPromoLinkFlowContainer : LinkFlowContainer
        {
            public SupporterPromoLinkFlowContainer(Action<SpriteText> defaultCreationParameters)
                : base(defaultCreationParameters)
            {
            }

            public new void AddLink(string text, string url, Action<SpriteText> creationParameters) =>
                AddInternal(new SupporterPromoLinkCompiler(AddText(text, creationParameters)) { Url = url });

            private class SupporterPromoLinkCompiler : DrawableLinkCompiler
            {
                [Resolved(CanBeNull = true)]
                private OsuGame game { get; set; }

                public string Url;

                public SupporterPromoLinkCompiler(IEnumerable<Drawable> parts)
                    : base(parts)
                {
                    RelativeSizeAxes = Axes.Both;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colour)
                {
                    TooltipText = Url;
                    Action = () => game?.HandleLink(Url);
                    IdleColour = colour.PinkDark;
                    HoverColour = Color4.White;
                }
            }
        }
    }
}
