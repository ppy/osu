﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;
using System;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class DrumRoll : TaikoHitObject, IHasEndTime
    {
        /// <summary>
        /// Drum roll distance that results in a duration of 1 speed-adjusted beat length.
        /// </summary>
        private const float base_distance = 100;

        public double EndTime => StartTime + Duration;

        public double Duration { get; set; }

        /// <summary>
        /// Numer of ticks per beat length.
        /// </summary>
        public int TickRate = 1;

        /// <summary>
        /// Number of drum roll ticks required for a "Good" hit.
        /// </summary>
        public double RequiredGoodHits { get; protected set; }

        /// <summary>
        /// Number of drum roll ticks required for a "Great" hit.
        /// </summary>
        public double RequiredGreatHits { get; protected set; }

        /// <summary>
        /// The length (in milliseconds) between ticks of this drumroll.
        /// <para>Half of this value is the hit window of the ticks.</para>
        /// </summary>
        private double tickSpacing = 100;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);

            tickSpacing = timingPoint.BeatLength / TickRate;

            RequiredGoodHits = NestedHitObjects.Count * Math.Min(0.15, 0.05 + 0.10 / 6 * difficulty.OverallDifficulty);
            RequiredGreatHits = NestedHitObjects.Count * Math.Min(0.30, 0.10 + 0.20 / 6 * difficulty.OverallDifficulty);
        }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            createTicks();
        }

        private void createTicks()
        {
            if (tickSpacing == 0)
                return;

            bool first = true;
            for (double t = StartTime; t < EndTime + tickSpacing / 2; t += tickSpacing)
            {
                AddNested(new DrumRollTick
                {
                    FirstTick = first,
                    TickSpacing = tickSpacing,
                    StartTime = t,
                    IsStrong = IsStrong
                });

                first = false;
            }
        }
    }
}
