// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Audio;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Swell : TaikoHitObject, IHasEndTime
    {
        public double EndTime => StartTime + Duration;

        public double Duration { get; set; }

        /// <summary>
        /// The number of hits required to complete the swell successfully.
        /// </summary>
        public int RequiredHits = 10;

        public List<DrumSampleMapping> ProgressionSamples = new List<DrumSampleMapping>();

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            var progressionSamplePoints = new[] { controlPointInfo.SamplePointAt(StartTime) }
                .Concat(controlPointInfo.SamplePoints.Where(p => p.Time > StartTime && p.Time <= EndTime));

            foreach (var point in progressionSamplePoints)
                ProgressionSamples.Add(new DrumSampleMapping(point));
        }
    }
}
