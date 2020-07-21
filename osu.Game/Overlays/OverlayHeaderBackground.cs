// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays
{
    public class OverlayHeaderBackground : CompositeDrawable
    {
        public OverlayHeaderBackground(string textureName)
        {
            Height = 80;
            RelativeSizeAxes = Axes.X;
            Masking = true;
            InternalChild = new Background(textureName);
        }

        private class Background : Sprite
        {
            private readonly string textureName;

            public Background(string textureName)
            {
                this.textureName = textureName;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(textureName);
            }
        }
    }
}
