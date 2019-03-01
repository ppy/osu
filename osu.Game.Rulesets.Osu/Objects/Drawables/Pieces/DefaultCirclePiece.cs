// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class DefaultCirclePiece : Container
    {
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get(@"Play/osu/disc"),
                },
                new TrianglesPiece
                {
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingMode.Additive,
                    Alpha = 0.5f,
                }
            };
        }
    }
}
