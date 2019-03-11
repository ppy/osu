// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class ScreenTitle : CompositeDrawable, IHasAccentColour
    {
        private readonly SpriteIcon iconSprite;
        private readonly OsuSpriteText titleText, pageText;

        protected FontAwesome Icon
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
                iconSprite = new SpriteIcon
                {
                    Size = new Vector2(25),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Right = 10 },
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
            };
        }
    }
}
