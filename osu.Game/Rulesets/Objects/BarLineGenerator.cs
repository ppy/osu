// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects
{
    public class BarLineGenerator<TBarLine>
        where TBarLine : class, IBarLine, new()
    {
        /// <summary>
        /// The generated bar lines.
        /// </summary>
        public readonly List<TBarLine> BarLines = new List<TBarLine>();

        /// <summary>
        /// Constructs and generates bar lines for provided beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to generate bar lines for.</param>
        public BarLineGenerator(IBeatmap beatmap)
        {
            if (beatmap.HitObjects.Count == 0)
                return;

            HitObject lastObject = beatmap.HitObjects.Last();
            double lastHitTime = 1 + ((lastObject as IHasEndTime)?.EndTime ?? lastObject.StartTime);

            var timingPoints = beatmap.ControlPointInfo.TimingPoints;

            if (timingPoints.Count == 0)
                return;

            for (int i = 0; i < timingPoints.Count; i++)
            {
                TimingControlPoint currentTimingPoint = timingPoints[i];
                int currentBeat = 0;

                // Stop on the beat before the next timing point, or if there is no next timing point stop slightly past the last object
                double endTime = i < timingPoints.Count - 1 ? timingPoints[i + 1].Time - currentTimingPoint.BeatLength : lastHitTime + currentTimingPoint.BeatLength * (int)currentTimingPoint.TimeSignature;

                double barLength = currentTimingPoint.BeatLength * (int)currentTimingPoint.TimeSignature;

                for (double t = currentTimingPoint.Time; Precision.DefinitelyBigger(endTime, t); t += barLength, currentBeat++)
                {
                    BarLines.Add(new TBarLine
                    {
                        StartTime = t,
                        Major = currentBeat % (int)currentTimingPoint.TimeSignature == 0
                    });
                }
            }
        }
    }
}
