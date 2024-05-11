// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTournamentHeaderText : CompositeDrawable
    {
        public DrawableTournamentHeaderText(bool center = true)
        {
            InternalChild = new TextSprite
            {
                Anchor = center ? Anchor.Centre : Anchor.TopLeft,
                Origin = center ? Anchor.Centre : Anchor.TopLeft,
            };

            Height = 22;
            RelativeSizeAxes = Axes.X;
        }

        private partial class TextSprite : Sprite
        {
            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fit;

                Texture = textures.Get("header-text");
            }
        }
    }
}
