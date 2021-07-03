// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;
using osuTK;
using static osu.Game.Rulesets.Osu.Replays.Movers.MoverUtilExtensions;

namespace osu.Game.Rulesets.Osu.Replays.Movers
{
    public class PippiMover : DanceMover
    {
        public override Vector2 Update(double time) => ApplyPippiOffset(Interpolation.ValueAt(time, StartPos, EndPos, StartTime, EndTime), time);
    }
}
