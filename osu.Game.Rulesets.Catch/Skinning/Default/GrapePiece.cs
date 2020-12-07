// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public class GrapePiece : PulpFormation
    {
        public GrapePiece()
        {
            InternalChildren = new Drawable[]
            {
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(SMALL_PULP),
                    Y = -0.25f,
                },
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(LARGE_PULP_3),
                    Position = PositionAt(0, DISTANCE_FROM_CENTRE_3),
                },
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(LARGE_PULP_3),
                    Position = PositionAt(120, DISTANCE_FROM_CENTRE_3),
                },
                new Pulp
                {
                    Size = new Vector2(LARGE_PULP_3),
                    AccentColour = { BindTarget = AccentColour },
                    Position = PositionAt(240, DISTANCE_FROM_CENTRE_3),
                },
            };
        }
    }
}
