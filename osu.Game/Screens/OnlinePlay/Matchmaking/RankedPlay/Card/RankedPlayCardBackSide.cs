// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    public partial class RankedPlayCardBackSide : CompositeDrawable
    {
        public RankedPlayCardBackSide()
        {
            Size = RankedPlayCard.SIZE;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, TextureStore textures)
        {
            Masking = true;
            CornerRadius = RankedPlayCard.CORNER_RADIUS;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour =
                        ColourInfo.GradientVertical(
                            colourProvider.Background3,
                            colourProvider.Background4),
                },
                new TrianglesV2
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                    SpawnRatio = 1.2f,
                    Velocity = 0.1f,
                },
                new Sprite
                {
                    Texture = textures.Get(@"Menu/logo"),
                    Size = new Vector2(32),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }
    }
}
