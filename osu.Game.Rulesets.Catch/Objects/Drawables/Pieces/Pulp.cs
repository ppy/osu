// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public class Pulp : Circle
    {
        public Pulp()
        {
            RelativePositionAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingParameters.Additive;
            Colour = Colour4.White.Opacity(0.9f);
        }

        public readonly Bindable<Colour4> AccentColour = new Bindable<Colour4>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AccentColour.BindValueChanged(updateAccentColour, true);
        }

        private void updateAccentColour(ValueChangedEvent<Colour4> colour)
        {
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Radius = Size.X / 2,
                Colour = colour.NewValue.Darken(0.2f).Opacity(0.75f)
            };
        }
    }
}
