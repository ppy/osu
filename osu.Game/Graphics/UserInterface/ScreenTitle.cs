// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class ScreenTitle : CompositeDrawable, IHasAccentColour
    {
        public const float ICON_WIDTH = ICON_SIZE + icon_spacing;

        public const float ICON_SIZE = 25;

        private SpriteIcon iconSprite;
        private readonly OsuSpriteText titleText, pageText;

        private const float icon_spacing = 10;

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
                    Spacing = new Vector2(icon_spacing, 0),
                    Children = new[]
                    {
                        CreateIcon(),
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(6, 0),
                            Children = new[]
                            {
                                titleText = new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Light),
                                },
                                pageText = new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Light),
                                }
                            }
                        }
                    }
                },
            };
        }
    }
}
