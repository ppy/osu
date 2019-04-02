// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        private readonly SpriteIcon iconSprite;
        private readonly OsuSpriteText titleText, pageText;

        protected IconUsage Icon
        {
            get => iconSprite.Icon;
            set => iconSprite.Icon = value;
        }

        protected string Title
        {
            get => titleText.Text;
            set => titleText.Text = value;
        }

        protected string Section
        {
            get => pageText.Text;
            set => pageText.Text = value;
        }

        public Color4 AccentColour
        {
            get => pageText.Colour;
            set => pageText.Colour = value;
        }

        protected ScreenTitle()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        iconSprite = new SpriteIcon
                        {
                            Size = new Vector2(25),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(6, 0),
                            Children = new[]
                            {
                                titleText = new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 25),
                                },
                                pageText = new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 25),
                                }
                            }
                        }
                    }
                },
            };
        }
    }
}
