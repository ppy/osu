// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A custom icon class for use with <see cref="ScreenTitle.CreateIcon()"/>
    /// </summary>
    public class ScreenTitleIcon : CompositeDrawable
    {
        private const float circle_allowance = 0.8f;

        private readonly string icon;

        public ScreenTitleIcon(string icon)
        {
            this.icon = icon;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, OsuColour colours)
        {
            Size = new Vector2(ScreenTitle.ICON_SIZE / circle_allowance);

            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    Masking = true,
                    BorderColour = colours.Violet,
                    BorderThickness = 3,
                    MaskingSmoothness = 1,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(icon),
                            Size = new Vector2(circle_allowance),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Violet,
                            Alpha = 0,
                            AlwaysPresent = true,
                        },
                    }
                },
            };
        }
    }
}
