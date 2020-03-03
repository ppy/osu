// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class ScreenTitle : CompositeDrawable, IHasAccentColour
    {
        public const float ICON_WIDTH = ICON_SIZE + spacing;

        public const float ICON_SIZE = 30; // osu-web uses 40, but the images there aren't cropped edge-to-edge
        private const float spacing = 10;
        private const float text_padding = 17.5f; // 15px padding + 2.5px compensation for line-height

        private SpriteIcon iconSprite;
        private readonly OsuSpriteText titleText, pageText;
        protected readonly Circle Separator;

        protected IconUsage Icon
        {
            set
            {
                if (iconSprite == null)
                    throw new InvalidOperationException($"Cannot use {nameof(Icon)} with a custom {nameof(CreateIcon)} function.");

                iconSprite.Icon = value;
            }
        }

        protected string Title
        {
            set => titleText.Text = value;
        }

        protected string Section
        {
            set => pageText.Text = value;
        }

        public Color4 AccentColour
        {
            get => pageText.Colour;
            set => pageText.Colour = value;
        }

        public Color4 SeparatorColour
        {
            get => Separator.Colour;
            set => Separator.Colour = value;
        }

        protected virtual Drawable CreateIcon() => iconSprite = new SpriteIcon
        {
            Size = new Vector2(ICON_SIZE),
        };

        protected ScreenTitle()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(spacing, 0),
                    Direction = FillDirection.Horizontal,
                    Children = new[]
                    {
                        CreateIcon().With(t =>
                        {
                            t.Anchor = Anchor.Centre;
                            t.Origin = Anchor.Centre;
                            t.Margin = new MarginPadding { Horizontal = 5 }; // compensates for osu-web sprites having around 5px of whitespace on each side
                        }),
                        titleText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                            Margin = new MarginPadding { Vertical = text_padding }
                        },
                        Separator = new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(5),
                            Colour = Color4.Gray,
                            Margin = new MarginPadding { Top = 3 } // compensation for osu-web using a font here making the circle appear a bit lower
                        },
                        pageText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 20),
                            Margin = new MarginPadding { Vertical = text_padding }
                        }
                    }
                },
            };
        }
    }
}
