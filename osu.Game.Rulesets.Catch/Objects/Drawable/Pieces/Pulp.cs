// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable.Pieces
{
    public class Pulp : Circle, IHasAccentColour
    {
        public Pulp()
        {
            RelativePositionAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingParameters.Additive;
            Colour = Color4.White.Opacity(0.9f);
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                if (IsLoaded) updateAccentColour();
            }
        }

        private void updateAccentColour()
        {
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Radius = Size.X / 2,
                Colour = accentColour.Darken(0.2f).Opacity(0.75f)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateAccentColour();
        }
    }
}
