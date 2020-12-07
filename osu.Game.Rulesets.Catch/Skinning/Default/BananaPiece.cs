// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public class BananaPiece : PulpFormation
    {
        public BananaPiece()
        {
            InternalChildren = new Drawable[]
            {
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(SMALL_PULP),
                    Y = -0.3f
                },
                new Pulp
                {
                    AccentColour = { BindTarget = AccentColour },
                    Size = new Vector2(LARGE_PULP_4 * 0.8f, LARGE_PULP_4 * 2.5f),
                    Y = 0.05f,
                },
            };
        }
    }
}
