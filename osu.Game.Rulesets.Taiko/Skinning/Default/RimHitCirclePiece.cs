// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Taiko.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Default
{
    public partial class RimHitCirclePiece : CirclePiece
    {
        public RimHitCirclePiece()
        {
            Add(new RimHitSymbolPiece());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AccentColour = Hit.COLOUR_RIM;
        }

        /// <summary>
        /// The symbol used for rim hit pieces.
        /// </summary>
        public partial class RimHitSymbolPiece : CircularContainer
        {
            public RimHitSymbolPiece()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(SYMBOL_SIZE);

                BorderThickness = SYMBOL_BORDER;
                BorderColour = Color4.White;
                Masking = true;
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    }
                };
            }
        }
    }
}
