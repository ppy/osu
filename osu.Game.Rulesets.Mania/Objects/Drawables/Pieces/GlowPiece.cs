// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Objects.Drawables.Pieces
{
    public class GlowPiece : CompositeDrawable, IHasAccentColour
    {
        private const float glow_alpha = 0.7f;
        private const float glow_radius = 5;

        public GlowPiece()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;

            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                AlwaysPresent = true
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateGlow();
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;

                updateGlow();
            }
        }

        private void updateGlow()
        {
            if (!IsLoaded)
                return;

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = AccentColour.Opacity(glow_alpha),
                Radius = glow_radius,
                Hollow = true
            };
        }
    }
}
