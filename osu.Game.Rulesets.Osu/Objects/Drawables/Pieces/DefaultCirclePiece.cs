// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Threading.Tasks;
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
        private async Task load(TextureStore textures)
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = await textures.GetAsync(@"Play/osu/disc"),
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
