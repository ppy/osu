// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using static osu.Game.Rulesets.Osu.Replays.Movers.MoverUtilExtensions;

namespace osu.Game.Rulesets.Osu.Replays.Movers
{
    public class HalfCircleMover : Mover
    {
        private Vector2 middle = Vector2.Zero;
        private float radius;
        private float ang;

        public override void OnObjChange()
        {
            middle = (StartPos + EndPos) / 2;
            ang = StartPos.AngleRV(middle);
            radius = Vector2.Distance(middle, Start.StackedPosition);
        }

        public override Vector2 Update(double time) => middle + V2FromRad(ang + T(time) * MathF.PI, radius);
    }
}
