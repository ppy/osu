// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// Represents a hit object which requires pressing, holding, and releasing a key.
    /// </summary>
    public class HoldNote : ManiaHitObject, IHasEndTime
    {
        public double Duration { get; set; }
        public double EndTime => StartTime + Duration;

        private Note headNote;
        public Note HeadNote => headNote ?? (headNote = new Note { StartTime = StartTime });

        private Note tailNote;
        public Note TailNote => tailNote ?? (tailNote = new HoldNoteTail { StartTime = EndTime });

        /// <summary>
        /// The length (in milliseconds) between ticks of this hold.
        /// </summary>
        private double tickSpacing = 50;

        public IEnumerable<HoldNoteTick> Ticks => ticks ?? (ticks = createTicks());
        private List<HoldNoteTick> ticks;

        private List<HoldNoteTick> createTicks()
        {
            var ret = new List<HoldNoteTick>();

            if (tickSpacing == 0)
                return ret;

            for (double t = StartTime + HeadNote.HitWindows.Great / 2; t <= EndTime - TailNote.HitWindows.Great / 2; t+= tickSpacing)
            {
                ret.Add(new HoldNoteTick
                {
                    StartTime = t
                });
            }

            return ret;
        }
    }
}
