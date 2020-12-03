// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public class RaspberryPiece : PulpFormation
    {
        public RaspberryPiece()
        {
            InternalChildren = new Drawable[]
            {
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(SMALL_PULP),
                    Y = -0.34f,
                },
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(LARGE_PULP_4),
                    Position = PositionAt(0, DISTANCE_FROM_CENTRE_4),
                },
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(LARGE_PULP_4),
                    Position = PositionAt(90, DISTANCE_FROM_CENTRE_4),
                },
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(LARGE_PULP_4),
                    Position = PositionAt(180, DISTANCE_FROM_CENTRE_4),
                },
                new Pulp
                {
                    Size = new Vector2(LARGE_PULP_4),
                    AccentColour = { BindTarget = AccentColour },
                    Position = PositionAt(270, DISTANCE_FROM_CENTRE_4),
                },
            };
        }
    }
}
