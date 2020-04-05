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

        public const float ICON_SIZE = 25;
        private const float spacing = 6;
        private const int text_offset = 2;

        private SpriteIcon iconSprite;
        private readonly Circle separator;
        private readonly OsuSpriteText titleText, pageText;

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
            set => titleText.Text = value.ToLower();
        }

        protected string Section
        {
            set
            {
                pageText.Text = value.ToLower();
                separator.Alpha = string.IsNullOrEmpty(value) ? 0 : 1;
            }
        }

        public Color4 AccentColour
        {
            get => pageText.Colour;
            set => pageText.Colour = value;
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
                        }),
                        titleText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 30, weight: FontWeight.Bold),
                            Margin = new MarginPadding { Bottom = text_offset }
                        },
                        separator = new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(4),
                            Colour = Color4.Gray,
                            Alpha = 0,
                        },
                        pageText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 30),
                            Margin = new MarginPadding { Bottom = text_offset }
                        }
                    }
                },
            };
        }
    }
}
