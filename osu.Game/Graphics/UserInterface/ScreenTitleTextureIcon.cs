// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A custom icon class for use with <see cref="ScreenTitle.CreateIcon()"/> based off a texture resource.
    /// </summary>
    public class ScreenTitleTextureIcon : CompositeDrawable
    {
        private readonly string textureName;

        public ScreenTitleTextureIcon(string textureName)
        {
            this.textureName = textureName;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Size = new Vector2(ScreenTitle.ICON_SIZE);

            InternalChild = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Texture = textures.Get(textureName),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fit
            };
        }
    }
}
