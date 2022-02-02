// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Taiko.Objects;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Default
{
    public class CentreHitCirclePiece : CirclePiece
    {
        public CentreHitCirclePiece()
        {
            Add(new CentreHitSymbolPiece());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AccentColour = Hit.COLOUR_CENTRE;
        }

        /// <summary>
        /// The symbol used for centre hit pieces.
        /// </summary>
        public class CentreHitSymbolPiece : Container
        {
            public CentreHitSymbolPiece()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(SYMBOL_SIZE);
                Padding = new MarginPadding(SYMBOL_BORDER);

                Children = new[]
                {
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new[] { new Box { RelativeSizeAxes = Axes.Both } }
                    }
                };
            }
        }
    }
}
