// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Catch
{
    /// <summary>
    /// Legacy osu!catch Spinner-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSpinner : HitObject, IHasEndTime
    {
        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;
    }
}
