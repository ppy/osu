// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    internal sealed class ConvertHold : HitObject, IHasXPosition, IHasEndTime
    {
        public float X { get; set; }

        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;

        protected override HitWindows CreateHitWindows() => null;
    }
}
