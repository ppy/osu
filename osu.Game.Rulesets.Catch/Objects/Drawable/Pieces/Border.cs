// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable.Pieces
{
    public class Border : Circle
    {
        public Border(float glowRadius, Vector2 size, float borderThickness, Color4 accentColour, bool hyperDash)
        {
            EdgeEffect = new EdgeEffectParameters
            {
                Hollow = !hyperDash,
                Type = EdgeEffectType.Glow,
                Radius = glowRadius,
                Colour = hyperDash ? Color4.Red : accentColour.Darken(1).Opacity(0.6f)
            };
            Size = size;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            BorderColour = Color4.White;
            BorderThickness = borderThickness;
            Children = new Framework.Graphics.Drawable[]
            {
                new Box
                {
                    AlwaysPresent = true,
                    Colour = accentColour,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}
