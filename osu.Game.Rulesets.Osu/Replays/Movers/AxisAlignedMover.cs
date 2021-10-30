// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Replays.Movers
{
    public class AxisAlignedMover : Mover
    {
        private SliderPath path;

        public override void OnObjChange()
        {
            var midP = MathF.Abs((EndPos - StartPos).X) < MathF.Abs((EndPos - EndPos).X)
                ? new Vector2(StartX, EndY)
                : new Vector2(EndX, StartY);

            path = new SliderPath(new[]
            {
                new PathControlPoint(StartPos, PathType.Linear),
                new PathControlPoint(midP),
                new PathControlPoint(EndPos)
            });
        }

        public override Vector2 Update(double time) => path.PositionAt(T(time));
    }
}
