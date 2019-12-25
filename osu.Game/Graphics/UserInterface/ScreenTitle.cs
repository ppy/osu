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
        public const float ICON_SIZE = 30;
        private const int text_offset = 2;

        private SpriteIcon iconSprite;
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
                    Spacing = new Vector2(6, 0),
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
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                            Margin = new MarginPadding { Bottom = text_offset }
                        },
                        new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            Size = new Vector2(4),
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Gray,
                            }
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
    }
}
