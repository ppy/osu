// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public class PearPiece : PulpFormation
    {
        public PearPiece()
        {
            InternalChildren = new Drawable[]
            {
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(SMALL_PULP),
                    Y = -0.33f,
                },
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(LARGE_PULP_3),
                    Position = PositionAt(60, DISTANCE_FROM_CENTRE_3),
                },
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(LARGE_PULP_3),
                    Position = PositionAt(180, DISTANCE_FROM_CENTRE_3),
                },
                new Pulp
                {
                    Size = new Vector2(LARGE_PULP_3),
                    AccentColour = { BindTarget = AccentColour },
                    Position = PositionAt(300, DISTANCE_FROM_CENTRE_3),
                },
            };
        }
    }
}
