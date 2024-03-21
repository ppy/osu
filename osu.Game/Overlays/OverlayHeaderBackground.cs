// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays
{
    public partial class OverlayHeaderBackground : CompositeDrawable
    {
        public OverlayHeaderBackground(string textureName)
        {
            Height = 80;
            RelativeSizeAxes = Axes.X;
            Masking = true;
            InternalChild = new DelayedLoadWrapper(() => new Background(textureName));
        }

        private partial class Background : Sprite
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
            private void load(LargeTextureStore textures)
            {
                Texture = textures.Get(textureName);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                this.FadeInFromZero(500, Easing.OutQuint);
            }
        }
    }
}
