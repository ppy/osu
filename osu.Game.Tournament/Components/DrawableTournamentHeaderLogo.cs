// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Tournament.Components
{
    public class DrawableTournamentHeaderLogo : CompositeDrawable
    {
        public DrawableTournamentHeaderLogo()
        {
            InternalChild = new LogoSprite();

            Height = 82;
            RelativeSizeAxes = Axes.X;
        }

        private class LogoSprite : Sprite
        {
            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fit;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Texture = textures.Get("header-logo");
            }
        }
    }
}
