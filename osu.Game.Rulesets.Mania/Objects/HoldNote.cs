// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Database;
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

        private Note head;
        /// <summary>
        /// The head note of the hold.
        /// </summary>
        public Note Head => head ?? (head = new Note { StartTime = StartTime });

        private Note tail;
        /// <summary>
        /// The tail note of the hold.
        /// </summary>
        public Note Tail => tail ?? (tail = new TailNote { StartTime = EndTime });

        /// <summary>
        /// The time between ticks of this hold.
        /// </summary>
        private double tickSpacing = 50;

        public override void ApplyDefaults(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            tickSpacing = timingPoint.BeatLength / difficulty.SliderTickRate;
        }

        /// <summary>
        /// The scoring scoring ticks of the hold note.
        /// </summary>
        public IEnumerable<HoldNoteTick> Ticks => ticks ?? (ticks = createTicks());
        private List<HoldNoteTick> ticks;

        private List<HoldNoteTick> createTicks()
        {
            var ret = new List<HoldNoteTick>();

            if (tickSpacing == 0)
                return ret;

            for (double t = StartTime + Head.HitWindows.Great / 2; t <= EndTime - Tail.HitWindows.Great / 2; t+= tickSpacing)
            {
                ret.Add(new HoldNoteTick
                {
                    StartTime = t
                });
            }

            return ret;
        }

        /// <summary>
        /// The tail of the hold note.
        /// </summary>
        private class TailNote : Note
        {
            /// <summary>
            /// Lenience of release hit windows. This is to make cases where the hold note release
            /// is timed alongside presses of other hit objects less awkward.
            /// </summary>
            private const double release_window_lenience = 1.5;

            public override void ApplyDefaults(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
            {
                base.ApplyDefaults(controlPointInfo, difficulty);

                HitWindows *= release_window_lenience;
            }
        }
    }
}
